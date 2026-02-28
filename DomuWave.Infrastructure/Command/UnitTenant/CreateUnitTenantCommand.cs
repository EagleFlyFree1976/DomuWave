using DomuWave.Services.Dto.UnitTenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitTenant;

public class CreateUnitTenantCommand : BaseCommand, IQuery<UnitTenantReadDto>
{
    public CreateUnitTenantDto Dto { get; set; }

    public CreateUnitTenantCommand() { }

    public CreateUnitTenantCommand(int currentUserId) : base(currentUserId) { }
    public CreateUnitTenantCommand(int currentUserId, CreateUnitTenantDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
