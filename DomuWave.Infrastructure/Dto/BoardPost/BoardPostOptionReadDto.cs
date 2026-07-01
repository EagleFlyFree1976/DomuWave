namespace DomuWave.Services.Dto.BoardPost;

/// <summary>
/// Opzione di un sondaggio con il relativo esito.
/// VoteCount e Voters sono popolati solo se i risultati sono visibili
/// (l'utente ha votato o il sondaggio è chiuso). Voters resta vuoto se anonimo.
/// </summary>
public class BoardPostOptionReadDto
{
    public int    Id       { get; set; }
    public string Text     { get; set; } = string.Empty;
    public int    OrderKey { get; set; }
    public int    VoteCount { get; set; }
    public List<BoardPostVoterDto> Voters { get; set; } = new();
}

/// <summary>Votante mostrato nei sondaggi NON anonimi (a risultati visibili).</summary>
public class BoardPostVoterDto
{
    public long   UserId   { get; set; }
    public string FullName { get; set; } = string.Empty;
}
