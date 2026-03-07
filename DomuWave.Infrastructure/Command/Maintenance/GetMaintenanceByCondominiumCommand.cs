using DomuWave.Services.Dto.Maintenance;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Maintenance;

public class GetMaintenanceByCondominiumCommand : BaseCommand, IQuery<IList<MaintenanceReadDto>>
{
    public int CondominiumId { get; }

    public GetMaintenanceByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
