using DomuWave.Services.Dto.RealEstateUnit;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetRealEstateUnitsByCondominiumCommand : BaseCommand, IQuery<IList<RealEstateUnitReadDto>>
{
    public int CondominiumId { get; set; }

    public GetRealEstateUnitsByCondominiumCommand() { }

    public GetRealEstateUnitsByCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public GetRealEstateUnitsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
