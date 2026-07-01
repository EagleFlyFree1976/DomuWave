using DomuWave.Services.Dto.UserTenants;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

/// <summary>
/// Restituisce il profilo dell'utente corrente in uno specifico tenant.
/// Usato al cambio tenant lato frontend (ruolo per-tenant).
/// </summary>
public class GetTenantProfileCommand : BaseCommand, IQuery<TenantProfileDto>
{
    public Guid TenantId { get; set; }

    public GetTenantProfileCommand() { }

    public GetTenantProfileCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
