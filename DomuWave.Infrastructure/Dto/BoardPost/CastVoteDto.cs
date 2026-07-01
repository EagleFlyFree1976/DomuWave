namespace DomuWave.Services.Dto.BoardPost;

/// <summary>
/// Input per esprimere/modificare/revocare il voto su un sondaggio.
/// OptionIds: opzioni scelte (una per scelta singola, N per multipla, vuota = revoca totale).
/// </summary>
public class CastVoteDto
{
    public int       BoardPostId { get; set; }
    public List<int> OptionIds   { get; set; } = new();
}
