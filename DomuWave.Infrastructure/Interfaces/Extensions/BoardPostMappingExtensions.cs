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
        };
        dto.SetTraceInfo(entity);
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
        };
    }

    public static void ApplyUpdate(this BoardPost entity, UpdateBoardPostDto dto)
    {
        if (dto.Title != null) entity.Name        = dto.Title;
        if (dto.Body  != null) entity.Description = dto.Body;
        if (dto.IsPinned != null) entity.IsPinned = dto.IsPinned.Value;
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
