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
        var posts = await _boardPostService.GetByCondominiumAsync(command.CondominiumId, currentUser, ct).ConfigureAwait(false);

        var result = new List<BoardPostReadDto>();
        foreach (var post in posts)
        {
            var count = await session.Query<BoardPostComment>()
                .CountAsync(c => c.Post.Id == post.Id && !c.IsDeleted, ct).ConfigureAwait(false);
            result.Add(post.ToReadDto(count));
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

        var entity = command.Dto.ToEntity(condominium, condominium.Tenant, (long)currentUser.Id, currentUser.FullName);
        entity.Trace(currentUser);
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
