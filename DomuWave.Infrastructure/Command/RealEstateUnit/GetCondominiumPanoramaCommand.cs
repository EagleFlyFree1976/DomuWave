using DomuWave.Services.Dto.RealEstateUnit;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.RealEstateUnit;

public class GetCondominiumPanoramaCommand : BaseCommand, IQuery<IList<RealEstateUnitPanoramaDto>>
{
    public int CondominiumId { get; set; }

    public GetCondominiumPanoramaCommand() { }

    public GetCondominiumPanoramaCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
