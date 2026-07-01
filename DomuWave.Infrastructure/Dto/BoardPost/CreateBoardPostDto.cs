namespace DomuWave.Services.Dto.BoardPost;

public class CreateBoardPostDto
{
    public int    CondominiumId { get; set; }
    public string Title        { get; set; } = string.Empty;
    public string Body         { get; set; } = string.Empty;
    public bool   IsPinned     { get; set; }

    // ── Sondaggio (opzionale) ───────────────────────────────────────────────
    public bool         IsPoll        { get; set; }
    public bool         IsAnonymous   { get; set; }
    public bool         AllowMultiple { get; set; }
    public DateTime?    ClosesAt      { get; set; }
    // Testi delle opzioni (vuota per un post normale; min. 2 per un sondaggio).
    public List<string> Options       { get; set; } = new();
}
