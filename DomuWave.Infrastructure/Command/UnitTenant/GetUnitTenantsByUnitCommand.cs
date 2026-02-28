using DomuWave.Services.Dto.UnitTenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitTenant;

public class GetUnitTenantsByUnitCommand : BaseCommand, IQuery<IList<UnitTenantReadDto>>
{
    public int UnitId { get; set; }

    public GetUnitTenantsByUnitCommand() { }

    public GetUnitTenantsByUnitCommand(int currentUserId) : base(currentUserId) { }
    public GetUnitTenantsByUnitCommand(int currentUserId, int unitId) : base(currentUserId)
    {
        UnitId = unitId;
    }
}
