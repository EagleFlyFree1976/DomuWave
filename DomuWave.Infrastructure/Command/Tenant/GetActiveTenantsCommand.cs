using DomuWave.Services.Dto.Tenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant;

/// <summary>
/// Recupera tutti i tenant attivi, ordinati per nome.
/// </summary>
public class GetActiveTenantsCommand : BaseCommand, IQuery<IList<TenantReadDto>>
{
    public GetActiveTenantsCommand() { }

    public GetActiveTenantsCommand(int currentUserId) : base(currentUserId) { }
}
