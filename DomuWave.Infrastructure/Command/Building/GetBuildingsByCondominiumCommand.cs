using DomuWave.Services.Dto.Building;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Building;

public class GetBuildingsByCondominiumCommand : BaseCommand, IQuery<IList<BuildingReadDto>>
{
    public int CondominiumId { get; set; }

    public GetBuildingsByCondominiumCommand() { }
    public GetBuildingsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
