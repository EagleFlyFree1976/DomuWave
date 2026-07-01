using DomuWave.Services.Dto.BoardPost;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.BoardPost;

public class GetBoardPostsByCondominiumCommand : BaseCommand, IQuery<IList<BoardPostReadDto>>
{
    public int CondominiumId { get; set; }
    public GetBoardPostsByCondominiumCommand() { }
    public GetBoardPostsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}

public class CreateBoardPostCommand : BaseCommand, IQuery<BoardPostReadDto>
{
    public CreateBoardPostDto Dto { get; set; } = null!;
    public CreateBoardPostCommand() { }
    public CreateBoardPostCommand(int currentUserId, CreateBoardPostDto dto) : base(currentUserId)
        => Dto = dto;
}

public class UpdateBoardPostCommand : BaseCommand, IQuery<BoardPostReadDto>
{
    public int               Id  { get; set; }
    public UpdateBoardPostDto Dto { get; set; } = null!;
    public UpdateBoardPostCommand() { }
    public UpdateBoardPostCommand(int currentUserId, int id, UpdateBoardPostDto dto) : base(currentUserId) { Id = id; Dto = dto; }
}

public class DeleteBoardPostCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteBoardPostCommand() { }
    public DeleteBoardPostCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

public class GetBoardPostCommentsByPostCommand : BaseCommand, IQuery<IList<BoardPostCommentReadDto>>
{
    public int BoardPostId { get; set; }
    public GetBoardPostCommentsByPostCommand() { }
    public GetBoardPostCommentsByPostCommand(int currentUserId, int boardPostId) : base(currentUserId)
        => BoardPostId = boardPostId;
}

public class CreateBoardPostCommentCommand : BaseCommand, IQuery<BoardPostCommentReadDto>
{
    public CreateBoardPostCommentDto Dto { get; set; } = null!;
    public CreateBoardPostCommentCommand() { }
    public CreateBoardPostCommentCommand(int currentUserId, CreateBoardPostCommentDto dto) : base(currentUserId)
        => Dto = dto;
}

public class DeleteBoardPostCommentCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteBoardPostCommentCommand() { }
    public DeleteBoardPostCommentCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}

/// <summary>Esprime/modifica/revoca il voto del condòmino su un sondaggio. Ritorna il post aggiornato.</summary>
public class CastVoteCommand : BaseCommand, IQuery<BoardPostReadDto>
{
    public CastVoteDto Dto { get; set; } = null!;
    public CastVoteCommand() { }
    public CastVoteCommand(int currentUserId, CastVoteDto dto) : base(currentUserId) => Dto = dto;
}
