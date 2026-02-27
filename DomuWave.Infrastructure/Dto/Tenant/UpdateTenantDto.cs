namespace DomuWave.Services.Dto.Tenant;

/// <summary>
/// DTO per l'aggiornamento di un Tenant esistente.
/// </summary>
public class UpdateTenantDto
{
    public string Name { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; }
}
