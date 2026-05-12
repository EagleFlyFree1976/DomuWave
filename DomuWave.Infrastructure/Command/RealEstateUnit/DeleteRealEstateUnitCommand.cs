using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class DeleteRealEstateUnitCommand : BaseTenantRelatedCommand, IQuery<bool>
{
    public int UnitId { get; set; }

    public DeleteRealEstateUnitCommand() { }

    public DeleteRealEstateUnitCommand(int currentUserId) : base(currentUserId) { }
    public DeleteRealEstateUnitCommand(int currentUserId, Guid tenantId, int unitId) : base(currentUserId, tenantId)
    {
        UnitId = unitId;
    }
}
