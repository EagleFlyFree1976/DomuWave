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

    // ── Sondaggio ───────────────────────────────────────────────────────────
    public bool      IsPoll        { get; set; }
    public bool      IsAnonymous   { get; set; }
    public bool      AllowMultiple { get; set; }
    public DateTime? ClosesAt      { get; set; }
    // true se il sondaggio è scaduto (ClosesAt passato).
    public bool      IsClosed      { get; set; }
    // true se l'utente corrente ha già votato.
    public bool      HasVoted      { get; set; }
    // Id delle opzioni votate dall'utente corrente (più d'una se scelta multipla).
    public List<int> MyVotes       { get; set; } = new();
    // true se i risultati sono visibili (ha votato oppure è chiuso).
    public bool      ResultsVisible { get; set; }
    // Numero di votanti distinti.
    public int       TotalVoters   { get; set; }
    public List<BoardPostOptionReadDto> Options { get; set; } = new();
}
