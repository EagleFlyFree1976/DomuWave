using DomuWave.Services.Dto.Tenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant;

/// <summary>
/// Crea un nuovo Tenant. Restituisce il DTO dell'entità creata.
/// </summary>
public class CreateTenantCommand : BaseCommand, IQuery<TenantReadDto>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public bool IsActive { get; set; } = true;

    public CreateTenantCommand() { }

    public CreateTenantCommand(int currentUserId, CreateTenantDto dto) : base(currentUserId)
    {
        Name     = dto.Name;
        Code     = dto.Code;
        IsActive = dto.IsActive;
    }
}
