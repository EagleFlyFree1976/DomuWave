namespace DomuWave.Services.Dto.BoardPost;

public class CreateBoardPostDto
{
    public int    CondominiumId { get; set; }
    public string Title        { get; set; } = string.Empty;
    public string Body         { get; set; } = string.Empty;
    public bool   IsPinned     { get; set; }
}
