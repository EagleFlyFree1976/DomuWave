namespace DomuWave.Services.Dto.Tenant;

/// <summary>
/// DTO per la creazione di un nuovo Tenant.
/// </summary>
public class CreateTenantDto
{
    public string Name { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; } = true;
}
