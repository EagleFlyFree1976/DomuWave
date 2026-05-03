using DomuWave.Services.Dto.UnitTenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitTenant;

public class GetUnitTenantsByUnitCommand : BaseCommand, IQuery<IList<UnitTenantReadDto>>
{
    public int  UnitId   { get; set; }
    public Guid TenantId { get; set; }

    public GetUnitTenantsByUnitCommand() { }

    public GetUnitTenantsByUnitCommand(int currentUserId, int unitId, Guid tenantId) : base(currentUserId)
    {
        UnitId   = unitId;
        TenantId = tenantId;
    }
}
