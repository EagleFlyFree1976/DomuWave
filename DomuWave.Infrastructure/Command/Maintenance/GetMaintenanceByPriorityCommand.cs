using DomuWave.Services.Dto.Maintenance;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Maintenance;

public class GetMaintenanceByPriorityCommand : BaseCommand, IQuery<IList<MaintenanceReadDto>>
{
    public int    CondominiumId { get; }
    public string Priority      { get; }

    public GetMaintenanceByPriorityCommand(int currentUserId, int condominiumId, string priority) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Priority      = priority;
    }
}
