using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BoardPost;
using DomuWave.Services.Dto.BoardPost;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;
using Models = DomuWave.Services.Models;

namespace DomuWave.Services.Consumers.BoardPost;

public class GetBoardPostsByCondominiumCommandConsumer : InMemoryConsumerBase<GetBoardPostsByCondominiumCommand, IList<BoardPostReadDto>>
{
    private readonly IBoardPostService        _boardPostService;
    private readonly IBoardPostCommentService _commentService;
    private readonly IUserService             _userService;

    public GetBoardPostsByCondominiumCommandConsumer(ISessionFactoryProvider sp, IBoardPostService boardPostService, IBoardPostCommentService commentService, IUserService userService)
        : base(sp) { _boardPostService = boardPostService; _commentService = commentService; _userService = userService; }

    protected override async Task<IList<BoardPostReadDto>> Consume(GetBoardPostsByCondominiumCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var currentUserId = (long)currentUser.Id;
        var posts = await _boardPostService.GetByCondominiumAsync(command.CondominiumId, currentUser, ct).ConfigureAwait(false);

        var postIds = posts.Select(p => p.Id).ToList();

        // Conteggio commenti per tutti i post in un colpo solo (evita N+1).
        var commentCounts = (await session.Query<BoardPostComment>()
                .Where(c => postIds.Contains(c.Post.Id) && !c.IsDeleted)
                .Select(c => c.Post.Id)
                .ToListAsync(ct).ConfigureAwait(false))
            .GroupBy(id => id)
            .ToDictionary(g => g.Key, g => g.Count());

        // Voti dei soli sondaggi, in un'unica query, poi raggruppati in memoria.
        var pollIds = posts.Where(p => p.IsPoll).Select(p => p.Id).ToList();
        var votes = pollIds.Count == 0
            ? new List<BoardPostVote>()
            : await session.Query<BoardPostVote>()
                .Where(v => pollIds.Contains(v.Post.Id) && !v.IsDeleted)
                .ToListAsync(ct).ConfigureAwait(false);

        var votesByPost = votes.GroupBy(v => v.Post.Id).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<BoardPostReadDto>();
        foreach (var post in posts)
        {
            var count = commentCounts.TryGetValue(post.Id, out var cc) ? cc : 0;

            if (!post.IsPoll)
            {
                result.Add(post.ToReadDto(count));
                continue;
            }

            var postVotes = votesByPost.TryGetValue(post.Id, out var pv) ? pv : new List<BoardPostVote>();
            var votesByOption = postVotes
                .GroupBy(v => v.Option.Id)
                .ToDictionary(g => g.Key, g => g.Count());
            var votersByOption = postVotes
                .GroupBy(v => v.Option.Id)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(v => new BoardPostVoterDto { UserId = v.VoterUserId, FullName = v.VoterFullName }).ToList());
            var myOptionIds = postVotes
                .Where(v => v.VoterUserId == currentUserId)
                .Select(v => v.Option.Id)
                .Distinct()
                .ToList();
            var totalVoters = postVotes.Select(v => v.VoterUserId).Distinct().Count();

            result.Add(post.ToPollReadDto(count, votesByOption, votersByOption, myOptionIds, totalVoters));
        }
        return result;
    }
}

public class CreateBoardPostCommandConsumer : InMemoryConsumerBase<CreateBoardPostCommand, BoardPostReadDto>
{
    private readonly IBoardPostService _boardPostService;
    private readonly IUserService      _userService;

    public CreateBoardPostCommandConsumer(ISessionFactoryProvider sp, IBoardPostService boardPostService, IUserService userService)
        : base(sp) { _boardPostService = boardPostService; _userService = userService; }

    protected override async Task<BoardPostReadDto> Consume(CreateBoardPostCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Title))
            throw new ValidatorException("Il titolo è obbligatorio.");
        if (string.IsNullOrWhiteSpace(command.Dto.Body))
            throw new ValidatorException("Il testo è obbligatorio.");

        var condominium = await session.GetAsync<Models.Condominium>(command.Dto.CondominiumId, ct).ConfigureAwait(false)
                          ?? throw new NotFoundException("Condominio non trovato.");

        // Validazione sondaggio
        if (command.Dto.IsPoll)
        {
            var options = command.Dto.Options?.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList()
                          ?? new List<string>();
            if (options.Count < 2)
                throw new ValidatorException("Un sondaggio deve avere almeno 2 opzioni.");
            if (command.Dto.ClosesAt.HasValue && command.Dto.ClosesAt.Value <= DateTime.UtcNow)
                throw new ValidatorException("La data di chiusura del sondaggio deve essere futura.");
        }

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant, (long)currentUser.Id, currentUser.FullName);
        entity.Trace(currentUser);

        if (command.Dto.IsPoll)
        {
            var order = 0;
            foreach (var text in command.Dto.Options.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()))
            {
                var option = new Models.BoardPostOption
                {
                    Post     = entity,
                    Tenant   = condominium.Tenant,
                    Name     = text,
                    OrderKey = order++,
                };
                option.Trace(currentUser);
                entity.Options.Add(option);
            }
        }

        await session.SaveAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class UpdateBoardPostCommandConsumer : InMemoryConsumerBase<UpdateBoardPostCommand, BoardPostReadDto>
{
    private readonly IBoardPostService _boardPostService;
    private readonly IUserService      _userService;

    public UpdateBoardPostCommandConsumer(ISessionFactoryProvider sp, IBoardPostService boardPostService, IUserService userService)
        : base(sp) { _boardPostService = boardPostService; _userService = userService; }

    protected override async Task<BoardPostReadDto> Consume(UpdateBoardPostCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var entity = await _boardPostService.GetByIdAsync(command.Id, currentUser, ct).ConfigureAwait(false)
                     ?? throw new NotFoundException("Post non trovato.");
        entity.ApplyUpdate(command.Dto);
        entity.Trace(currentUser);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class DeleteBoardPostCommandConsumer : InMemoryConsumerBase<DeleteBoardPostCommand, bool>
{
    private readonly IBoardPostService _boardPostService;
    private readonly IUserService      _userService;

    public DeleteBoardPostCommandConsumer(ISessionFactoryProvider sp, IBoardPostService boardPostService, IUserService userService)
        : base(sp) { _boardPostService = boardPostService; _userService = userService; }

    protected override async Task<bool> Consume(DeleteBoardPostCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        return await _boardPostService.DeleteAsync(command.Id, currentUser, ct).ConfigureAwait(false);
    }
}

public class GetBoardPostCommentsByPostCommandConsumer : InMemoryConsumerBase<GetBoardPostCommentsByPostCommand, IList<BoardPostCommentReadDto>>
{
    private readonly IBoardPostCommentService _commentService;
    private readonly IUserService             _userService;

    public GetBoardPostCommentsByPostCommandConsumer(ISessionFactoryProvider sp, IBoardPostCommentService commentService, IUserService userService)
        : base(sp) { _commentService = commentService; _userService = userService; }

    protected override async Task<IList<BoardPostCommentReadDto>> Consume(GetBoardPostCommentsByPostCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var comments = await _commentService.GetByPostAsync(command.BoardPostId, currentUser, ct).ConfigureAwait(false);
        return comments.Select(c => c.ToReadDto()).ToList();
    }
}

public class CreateBoardPostCommentCommandConsumer : InMemoryConsumerBase<CreateBoardPostCommentCommand, BoardPostCommentReadDto>
{
    private readonly IBoardPostCommentService _commentService;
    private readonly IUserService             _userService;

    public CreateBoardPostCommentCommandConsumer(ISessionFactoryProvider sp, IBoardPostCommentService commentService, IUserService userService)
        : base(sp) { _commentService = commentService; _userService = userService; }

    protected override async Task<BoardPostCommentReadDto> Consume(CreateBoardPostCommentCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(command.Dto.Body))
            throw new ValidatorException("Il commento non può essere vuoto.");

        var post = await session.GetAsync<Models.BoardPost>(command.Dto.BoardPostId, ct).ConfigureAwait(false)
                   ?? throw new NotFoundException("Post non trovato.");

        var entity = command.Dto.ToEntity(post, post.Tenant, (long)currentUser.Id, currentUser.FullName);
        entity.Trace(currentUser);
        await session.SaveAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity.ToReadDto();
    }
}

public class DeleteBoardPostCommentCommandConsumer : InMemoryConsumerBase<DeleteBoardPostCommentCommand, bool>
{
    private readonly IBoardPostCommentService _commentService;
    private readonly IUserService             _userService;

    public DeleteBoardPostCommentCommandConsumer(ISessionFactoryProvider sp, IBoardPostCommentService commentService, IUserService userService)
        : base(sp) { _commentService = commentService; _userService = userService; }

    protected override async Task<bool> Consume(DeleteBoardPostCommentCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        return await _commentService.DeleteAsync(command.Id, currentUser, ct).ConfigureAwait(false);
    }
}

public class CastVoteCommandConsumer : InMemoryConsumerBase<CastVoteCommand, BoardPostReadDto>
{
    private readonly IBoardPostService _boardPostService;
    private readonly IUserService      _userService;

    public CastVoteCommandConsumer(ISessionFactoryProvider sp, IBoardPostService boardPostService, IUserService userService)
        : base(sp) { _boardPostService = boardPostService; _userService = userService; }

    protected override async Task<BoardPostReadDto> Consume(CastVoteCommand command, IMediationContext ctx, CancellationToken ct)
    {
        var currentUser   = await _userService.GetByIdAsync(command.CurrentUserId, ct).ConfigureAwait(false);
        var currentUserId = (long)currentUser.Id;

        var post = await session.Query<Models.BoardPost>()
            .FirstOrDefaultAsync(p => p.Id == command.Dto.BoardPostId && !p.IsDeleted, ct).ConfigureAwait(false)
            ?? throw new NotFoundException("Sondaggio non trovato.");

        if (!post.IsPoll)
            throw new ValidatorException("Questo post non è un sondaggio.");
        if (post.ClosesAt.HasValue && post.ClosesAt.Value < DateTime.UtcNow)
            throw new ValidatorException("Il sondaggio è scaduto.");

        var requested = (command.Dto.OptionIds ?? new List<int>()).Distinct().ToList();

        // Le opzioni richieste devono appartenere al sondaggio.
        var validOptionIds = post.Options.Where(o => !o.IsDeleted).Select(o => o.Id).ToHashSet();
        if (requested.Any(id => !validOptionIds.Contains(id)))
            throw new ValidatorException("Opzione di voto non valida.");

        if (!post.AllowMultiple && requested.Count > 1)
            throw new ValidatorException("Questo sondaggio ammette una sola scelta.");

        // Voti attivi dell'utente su questo sondaggio.
        var existing = await session.Query<BoardPostVote>()
            .Where(v => v.Post.Id == post.Id && v.VoterUserId == currentUserId && !v.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);
        var existingByOption = existing.ToDictionary(v => v.Option.Id, v => v);

        // Riconciliazione: soft-delete dei deselezionati, insert dei nuovi, invariati i confermati.
        foreach (var vote in existing.Where(v => !requested.Contains(v.Option.Id)))
        {
            vote.IsDeleted = true;
            vote.Trace(currentUser);
            await session.UpdateAsync(vote, ct).ConfigureAwait(false);
        }

        foreach (var optionId in requested.Where(id => !existingByOption.ContainsKey(id)))
        {
            var option = post.Options.First(o => o.Id == optionId);
            var vote = new BoardPostVote
            {
                Post          = post,
                Option        = option,
                Tenant        = post.Tenant,
                VoterUserId   = currentUserId,
                VoterFullName = currentUser.FullName,
            };
            vote.Trace(currentUser);
            await session.SaveAsync(vote, ct).ConfigureAwait(false);
        }

        await session.FlushAsync(ct).ConfigureAwait(false);

        // Ricostruisci il DTO con risultati visibili (l'utente ha appena votato).
        var commentCount = await session.Query<BoardPostComment>()
            .CountAsync(c => c.Post.Id == post.Id && !c.IsDeleted, ct).ConfigureAwait(false);

        var allVotes = await session.Query<BoardPostVote>()
            .Where(v => v.Post.Id == post.Id && !v.IsDeleted)
            .ToListAsync(ct).ConfigureAwait(false);

        var votesByOption = allVotes.GroupBy(v => v.Option.Id).ToDictionary(g => g.Key, g => g.Count());
        var votersByOption = allVotes.GroupBy(v => v.Option.Id)
            .ToDictionary(g => g.Key, g => g.Select(v => new BoardPostVoterDto { UserId = v.VoterUserId, FullName = v.VoterFullName }).ToList());
        var myOptionIds = allVotes.Where(v => v.VoterUserId == currentUserId).Select(v => v.Option.Id).Distinct().ToList();
        var totalVoters = allVotes.Select(v => v.VoterUserId).Distinct().Count();

        return post.ToPollReadDto(commentCount, votesByOption, votersByOption, myOptionIds, totalVoters);
    }
}
