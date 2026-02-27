using DomuWave.Services.Dto.Tenant;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant;

/// <summary>
/// Recupera tutti i tenant non eliminati.
/// </summary>
public class GetAllTenantsCommand : BaseCommand, IQuery<IList<TenantReadDto>>
{
    public GetAllTenantsCommand() { }

    public GetAllTenantsCommand(int currentUserId) : base(currentUserId) { }
}
