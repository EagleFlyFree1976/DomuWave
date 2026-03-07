using DomuWave.Services.Dto.Maintenance;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Maintenance;

public class CreateMaintenanceCommand : BaseCommand, IQuery<MaintenanceReadDto>
{
    public CreateMaintenanceDto Dto { get; }

    public CreateMaintenanceCommand(int currentUserId, CreateMaintenanceDto dto) : base(currentUserId)
        => Dto = dto;
}
