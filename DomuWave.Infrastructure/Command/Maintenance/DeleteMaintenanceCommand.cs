using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Maintenance;

public class DeleteMaintenanceCommand : BaseCommand, IQuery<bool>
{
    public int MaintenanceId { get; }

    public DeleteMaintenanceCommand(int currentUserId, int maintenanceId) : base(currentUserId)
        => MaintenanceId = maintenanceId;
}
