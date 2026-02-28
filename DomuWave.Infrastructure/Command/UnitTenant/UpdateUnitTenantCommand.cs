using DomuWave.Services.Dto.UnitTenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitTenant;

public class UpdateUnitTenantCommand : BaseCommand, IQuery<UnitTenantReadDto>
{
    public int Id { get; set; }
    public UpdateUnitTenantDto Dto { get; set; }

    public UpdateUnitTenantCommand() { }

    public UpdateUnitTenantCommand(int currentUserId) : base(currentUserId) { }
    public UpdateUnitTenantCommand(int currentUserId, int id, UpdateUnitTenantDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
