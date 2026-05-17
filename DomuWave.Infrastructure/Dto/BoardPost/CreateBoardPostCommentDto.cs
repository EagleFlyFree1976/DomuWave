namespace DomuWave.Services.Dto.BoardPost;

public class CreateBoardPostCommentDto
{
    public int    BoardPostId { get; set; }
    public string Body        { get; set; } = string.Empty;
}
