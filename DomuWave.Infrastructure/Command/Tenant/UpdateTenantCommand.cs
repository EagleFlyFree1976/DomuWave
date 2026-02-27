using DomuWave.Services.Dto.Tenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant;

/// <summary>
/// Aggiorna un Tenant esistente. Restituisce il DTO aggiornato,
/// oppure null se il tenant non esiste.
/// </summary>
public class UpdateTenantCommand : BaseCommand, IQuery<TenantReadDto>
{
    public Guid TenantId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; }

    public UpdateTenantCommand() { }

    public UpdateTenantCommand(int currentUserId, Guid tenantId, UpdateTenantDto dto) : base(currentUserId)
    {
        TenantId = tenantId;
        Name     = dto.Name;
        Code     = dto.Code;
        IsActive = dto.IsActive;
    }
}
