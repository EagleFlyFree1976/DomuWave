using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesByUnitCommand : BaseCommand, IQuery<IList<CondominiumFeeReadDto>>
{
    public int  UnitId   { get; set; }
    public Guid TenantId { get; set; }

    public GetFeesByUnitCommand() { }

    public GetFeesByUnitCommand(int currentUserId, int unitId, Guid tenantId) : base(currentUserId)
    {
        UnitId   = unitId;
        TenantId = tenantId;
    }
}
