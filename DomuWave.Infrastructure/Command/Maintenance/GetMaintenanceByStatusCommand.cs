using DomuWave.Services.Dto.Maintenance;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Maintenance;

public class GetMaintenanceByStatusCommand : BaseCommand, IQuery<IList<MaintenanceReadDto>>
{
    public int    CondominiumId { get; }
    public string Status        { get; }

    public GetMaintenanceByStatusCommand(int currentUserId, int condominiumId, string status) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Status        = status;
    }
}
