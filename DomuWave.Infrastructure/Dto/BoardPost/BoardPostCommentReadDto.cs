using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.BoardPost;

public class BoardPostCommentReadDto : TraceEntityDTO<int>
{
    public int    BoardPostId   { get; set; }
    public long   AuthorUserId  { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public string Body           { get; set; } = string.Empty;
}
