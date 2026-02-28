using DomuWave.Services.Dto.RealEstateUnit;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetRealEstateUnitsByTypeCommand : BaseCommand, IQuery<IList<RealEstateUnitReadDto>>
{
    public int CondominiumId { get; set; }
    public string UnitType { get; set; }

    public GetRealEstateUnitsByTypeCommand() { }

    public GetRealEstateUnitsByTypeCommand(int currentUserId) : base(currentUserId) { }
    public GetRealEstateUnitsByTypeCommand(int currentUserId, int condominiumId, string unitType) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        UnitType      = unitType;
    }
}
