namespace DomuWave.Services.Dto.Condominium;

/// <summary>
/// Condominio associato a un utente tramite il suo tenant.
/// Usato dalla pagina di gestione utenti (SuperAdmin).
/// </summary>
public class UserCondominiumDto
{
    public int    CondominiumId   { get; set; }
    public string CondominiumName { get; set; } = string.Empty;
    public string CondominiumCode { get; set; } = string.Empty;
    public Guid   TenantId        { get; set; }
    public string TenantName      { get; set; } = string.Empty;
}
