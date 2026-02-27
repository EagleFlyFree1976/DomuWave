using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetRealEstateUnitByIdCommand : BaseCommand, IQuery<Models.RealEstateUnit>
{
    public int UnitId { get; set; }

    public GetRealEstateUnitByIdCommand() { }

    public GetRealEstateUnitByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetRealEstateUnitByIdCommand(int currentUserId, int unitId) : base(currentUserId)
    {
        UnitId = unitId;
    }
}
