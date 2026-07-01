using CPQ.Core.Extensions;
using DomuWave.Services.Dto.BoardPost;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces.Extensions;

public static class BoardPostMappingExtensions
{
    public static BoardPostReadDto ToReadDto(this BoardPost entity, int commentCount = 0)
    {
        if (entity == null) return null!;
        var dto = new BoardPostReadDto
        {
            CondominiumId   = entity.Condominium?.Id ?? 0,
            CondominiumName = entity.Condominium?.Name ?? string.Empty,
            AuthorUserId    = entity.AuthorUserId,
            AuthorFullName  = entity.AuthorFullName,
            Title           = entity.Name,
            Body            = entity.Description,
            IsPinned        = entity.IsPinned,
            CommentCount    = commentCount,
            IsPoll          = entity.IsPoll,
            IsAnonymous     = entity.IsAnonymous,
            AllowMultiple   = entity.AllowMultiple,
            ClosesAt        = entity.ClosesAt,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    /// <summary>
    /// Proietta un sondaggio con il suo esito, applicando la gating dei risultati.
    /// </summary>
    /// <param name="votesByOption">Conteggio voti per OptionId (solo voti attivi).</param>
    /// <param name="votersByOption">Votanti per OptionId (UserId, FullName), per i sondaggi non anonimi.</param>
    /// <param name="myOptionIds">Opzioni votate dall'utente corrente.</param>
    /// <param name="totalVoters">Numero di votanti distinti.</param>
    public static BoardPostReadDto ToPollReadDto(
        this BoardPost entity,
        int commentCount,
        IReadOnlyDictionary<int, int> votesByOption,
        IReadOnlyDictionary<int, List<BoardPostVoterDto>> votersByOption,
        IReadOnlyCollection<int> myOptionIds,
        int totalVoters)
    {
        var dto = entity.ToReadDto(commentCount);

        var isClosed = entity.ClosesAt.HasValue && entity.ClosesAt.Value < DateTime.UtcNow;
        var hasVoted = myOptionIds.Count > 0;
        var resultsVisible = hasVoted || isClosed;

        dto.IsClosed       = isClosed;
        dto.HasVoted       = hasVoted;
        dto.MyVotes        = myOptionIds.ToList();
        dto.ResultsVisible = resultsVisible;
        dto.TotalVoters    = resultsVisible ? totalVoters : 0;

        dto.Options = entity.Options
            .Where(o => !o.IsDeleted)
            .OrderBy(o => o.OrderKey)
            .Select(o => new BoardPostOptionReadDto
            {
                Id       = o.Id,
                Text     = o.Name,
                OrderKey = o.OrderKey,
                // Conteggi solo a risultati visibili.
                VoteCount = resultsVisible && votesByOption.TryGetValue(o.Id, out var c) ? c : 0,
                // Votanti solo se risultati visibili E sondaggio non anonimo.
                Voters = resultsVisible && !entity.IsAnonymous && votersByOption.TryGetValue(o.Id, out var v)
                    ? v
                    : new List<BoardPostVoterDto>(),
            })
            .ToList();

        return dto;
    }

    public static BoardPost ToEntity(this CreateBoardPostDto dto, Condominium condominium, Tenant tenant, long authorUserId, string authorFullName)
    {
        if (dto == null) return null!;
        return new BoardPost
        {
            Condominium    = condominium,
            Tenant         = tenant,
            AuthorUserId   = authorUserId,
            AuthorFullName = authorFullName,
            Name        = dto.Title,
            Description = dto.Body,
            IsPinned       = dto.IsPinned,
            IsPoll         = dto.IsPoll,
            IsAnonymous    = dto.IsPoll && dto.IsAnonymous,
            AllowMultiple  = dto.IsPoll && dto.AllowMultiple,
            ClosesAt       = dto.IsPoll ? dto.ClosesAt : null,
        };
    }

    public static void ApplyUpdate(this BoardPost entity, UpdateBoardPostDto dto)
    {
        if (dto.Title != null) entity.Name        = dto.Title;
        if (dto.Body  != null) entity.Description = dto.Body;
        if (dto.IsPinned != null) entity.IsPinned = dto.IsPinned.Value;
        // ClosesAt è l'unico campo poll editabile dopo la creazione (solo per i sondaggi).
        if (entity.IsPoll && dto.ClosesAt != null) entity.ClosesAt = dto.ClosesAt;
    }

    public static BoardPostCommentReadDto ToReadDto(this BoardPostComment entity)
    {
        if (entity == null) return null!;
        var dto = new BoardPostCommentReadDto
        {
            BoardPostId    = entity.Post?.Id ?? 0,
            AuthorUserId   = entity.AuthorUserId,
            AuthorFullName = entity.AuthorFullName,
            Body           = entity.Name,
        };
        dto.SetTraceInfo(entity);
        return dto;
    }

    public static BoardPostComment ToEntity(this CreateBoardPostCommentDto dto, BoardPost post, Tenant tenant, long authorUserId, string authorFullName)
    {
        if (dto == null) return null!;
        return new BoardPostComment
        {
            Post           = post,
            Tenant         = tenant,
            AuthorUserId   = authorUserId,
            AuthorFullName = authorFullName,
            Name           = dto.Body,
        };
    }
}
