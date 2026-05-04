using DomuWave.Services.Dto.Building;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Building;

public class CreateBuildingCommand : BaseCommand, IQuery<BuildingReadDto>
{
    public CreateBuildingDto Dto { get; set; }

    public CreateBuildingCommand() { }
    public CreateBuildingCommand(int currentUserId, CreateBuildingDto dto) : base(currentUserId)
        => Dto = dto;
}
