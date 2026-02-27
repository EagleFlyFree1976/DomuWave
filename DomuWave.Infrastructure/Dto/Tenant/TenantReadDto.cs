using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Tenant;

/// <summary>
/// DTO di lettura per il Tenant.
/// Corrisponde alla proiezione del modello <see cref="Models.Tenant"/> con i campi di audit.
/// </summary>
public class TenantReadDto : TraceEntityDTO<Guid>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; }

 
    public string Description { get; set; }

    public bool IsPrimary { get; set; }
    public long OwnerId { get; set; }
}
