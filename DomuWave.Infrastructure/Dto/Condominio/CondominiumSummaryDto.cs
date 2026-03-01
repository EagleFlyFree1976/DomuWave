namespace DomuWave.Services.Dto.Condominium;

/// <summary>
/// Riepilogo di un condominio restituito nella risposta di login per gli utenti Condomino.
/// </summary>
public class CondominiumSummaryDto
{
    public int    CondominiumId   { get; set; }
    public string CondominiumName { get; set; } = string.Empty;
    public Guid   TenantId        { get; set; }
}
