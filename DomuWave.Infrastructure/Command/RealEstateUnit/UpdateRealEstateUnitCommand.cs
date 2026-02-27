using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class UpdateRealEstateUnitCommand : BaseCommand, IQuery<Models.RealEstateUnit>
{
    public int UnitId { get; set; }
    public Models.RealEstateUnit Entity { get; set; }

    public UpdateRealEstateUnitCommand() { }

    public UpdateRealEstateUnitCommand(int currentUserId) : base(currentUserId) { }
    public UpdateRealEstateUnitCommand(int currentUserId, int unitId, Models.RealEstateUnit entity) : base(currentUserId)
    {
        UnitId = unitId;
        Entity = entity;
    }
}
