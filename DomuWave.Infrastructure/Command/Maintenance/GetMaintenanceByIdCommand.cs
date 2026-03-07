using DomuWave.Services.Dto.Maintenance;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Maintenance;

public class GetMaintenanceByIdCommand : BaseCommand, IQuery<MaintenanceReadDto>
{
    public int MaintenanceId { get; }

    public GetMaintenanceByIdCommand(int currentUserId, int maintenanceId) : base(currentUserId)
        => MaintenanceId = maintenanceId;
}
