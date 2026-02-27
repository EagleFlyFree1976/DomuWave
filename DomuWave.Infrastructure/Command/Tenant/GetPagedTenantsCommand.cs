using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Helper;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant;

/// <summary>
/// Recupera i tenant con paginazione, ordinamento e filtri opzionali.
/// La logica di filtro risiede nel consumer e non nel controller.
/// </summary>
public class GetPagedTenantsCommand : BasePagedCommand, IQuery<PagedResult<TenantReadDto>>
{
    /// <summary>Testo di ricerca (filtra su Name). Null = nessun filtro.</summary>
    public string Search { get; set; }

    /// <summary>Filtra per stato attivo. Null = tutti.</summary>
    public bool? IsActive { get; set; }

    public GetPagedTenantsCommand() { }

    public GetPagedTenantsCommand(int currentUserId) : base(currentUserId) { }
}
