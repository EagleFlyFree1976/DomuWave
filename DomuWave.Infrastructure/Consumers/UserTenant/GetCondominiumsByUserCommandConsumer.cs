using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetCondominiumsByUserCommandConsumer
    : InMemoryConsumerBase<GetCondominiumsByUserCommand, IList<UserCondominiumDto>>
{
    private readonly IUserService _userService;

    public GetCondominiumsByUserCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<UserCondominiumDto>> Consume(
        GetCondominiumsByUserCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        // Recupera l'utente corrente per verificare il ruolo
        var currentUser = await _userService
            .GetByIdAsync((int)command.UserId, cancellationToken)
            .ConfigureAwait(false);

        // 1. Recupera i tenant dell'utente
        var userTenants = await session.Query<UserTenant>()
            .Where(ut => ut.UserId == command.UserId && ut.IsActive && !ut.IsDeleted)
            .Fetch(ut => ut.Tenant)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!userTenants.Any())
            return new List<UserCondominiumDto>();

        var tenantIds = userTenants.Select(ut => ut.Tenant.Id).ToHashSet();

        // 2. Per utenti con ruolo "condomino", filtra solo i condomini dove è proprietario
        IEnumerable<int> allowedCondominiumIds = null;
        if (currentUser != null && 
            string.Equals(currentUser.Role?.Code, "condomino", StringComparison.OrdinalIgnoreCase))
        {
            allowedCondominiumIds = await session.Query<UnitOwner>()
                .Where(o => o.UserId == command.UserId && o.IsActive && !o.IsDeleted)
                .Select(o => o.Unit.Condominium.Id)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Se l'utente condomino non ha unità, non deve vedere nessun condominio
            if (!allowedCondominiumIds.Any())
                return new List<UserCondominiumDto>();
        }

        // 3. Recupera i condomini del tenant
        var query = session.Query<Models.Condominium>()
            .Where(c => tenantIds.Contains(c.Tenant.Id) && !c.IsDeleted);

        // Applica il filtro per ruolo condomino
        if (allowedCondominiumIds != null)
        {
            query = query.Where(c => allowedCondominiumIds.Contains(c.Id));
        }

        var condominiums = await query
            .Fetch(c => c.Tenant)
            .OrderBy(c => c.Tenant.Id)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // 4. Mappa il risultato arricchito con il nome del tenant
        var tenantMap = userTenants.ToDictionary(ut => ut.Tenant.Id, ut => ut.Tenant.Name ?? string.Empty);

        return condominiums
            .Select(c => new UserCondominiumDto
            {
                CondominiumId   = c.Id,
                CondominiumName = c.Name ?? string.Empty,
                CondominiumCode = c.Code ?? string.Empty,
                TenantId        = c.Tenant.Id,
                TenantName      = tenantMap.TryGetValue(c.Tenant.Id, out var tn) ? tn : string.Empty,
            })
            .ToList();
    }
}
