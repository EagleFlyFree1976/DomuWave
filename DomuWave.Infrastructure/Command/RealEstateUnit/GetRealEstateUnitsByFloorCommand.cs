using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetRealEstateUnitsByFloorCommand : BaseCommand, IQuery<IList<Models.RealEstateUnit>>
{
    public int CondominiumId { get; set; }
    public int Floor { get; set; }

    public GetRealEstateUnitsByFloorCommand() { }

    public GetRealEstateUnitsByFloorCommand(int currentUserId) : base(currentUserId) { }
    public GetRealEstateUnitsByFloorCommand(int currentUserId, int condominiumId, int floor) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Floor = floor;
    }
}
