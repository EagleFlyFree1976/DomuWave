using DomuWave.Services.Dto.Building;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Building;

public class UpdateBuildingCommand : BaseCommand, IQuery<BuildingReadDto>
{
    public int Id { get; set; }
    public UpdateBuildingDto Dto { get; set; }

    public UpdateBuildingCommand() { }
    public UpdateBuildingCommand(int currentUserId, int id, UpdateBuildingDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
