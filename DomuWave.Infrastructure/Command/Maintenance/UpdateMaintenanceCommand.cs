using DomuWave.Services.Dto.Maintenance;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Maintenance;

public class UpdateMaintenanceCommand : BaseCommand, IQuery<MaintenanceReadDto>
{
    public int                 MaintenanceId { get; }
    public UpdateMaintenanceDto Dto          { get; }

    public UpdateMaintenanceCommand(int currentUserId, int maintenanceId, UpdateMaintenanceDto dto) : base(currentUserId)
    {
        MaintenanceId = maintenanceId;
        Dto           = dto;
    }
}
