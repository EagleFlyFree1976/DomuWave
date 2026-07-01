namespace DomuWave.Services.Dto.BoardPost;

public class UpdateBoardPostDto
{
    public string? Title    { get; set; }
    public string? Body     { get; set; }
    public bool?   IsPinned { get; set; }
    // Per i sondaggi: l'unico campo editabile dopo la creazione (proroga/chiusura).
    // Le opzioni e il tipo sono immutabili una volta pubblicato.
    public DateTime? ClosesAt { get; set; }
}
