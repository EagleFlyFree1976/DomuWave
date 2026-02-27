using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class DeleteRealEstateUnitCommand : BaseCommand, IQuery<bool>
{
    public int UnitId { get; set; }

    public DeleteRealEstateUnitCommand() { }

    public DeleteRealEstateUnitCommand(int currentUserId) : base(currentUserId) { }
    public DeleteRealEstateUnitCommand(int currentUserId, int unitId) : base(currentUserId)
    {
        UnitId = unitId;
    }
}
