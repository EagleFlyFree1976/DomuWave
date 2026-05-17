namespace DomuWave.Services.Dto.BoardPost;

public class UpdateBoardPostDto
{
    public string? Title    { get; set; }
    public string? Body     { get; set; }
    public bool?   IsPinned { get; set; }
}
