using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.BoardPost;

public class BoardPostReadDto : TraceEntityDTO<int>
{
    public int    CondominiumId   { get; set; }
    public string CondominiumName { get; set; } = string.Empty;
    public long   AuthorUserId   { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public string Title          { get; set; } = string.Empty;
    public string Body           { get; set; } = string.Empty;
    public bool   IsPinned       { get; set; }
    public int    CommentCount   { get; set; }
}
