using DomuWave.Services.Dto.Building;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Building;

public class GetBuildingByIdCommand : BaseCommand, IQuery<BuildingReadDto>
{
    public int Id { get; set; }

    public GetBuildingByIdCommand() { }
    public GetBuildingByIdCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
