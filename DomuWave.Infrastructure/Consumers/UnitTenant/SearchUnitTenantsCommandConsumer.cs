using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitTenant;
using DomuWave.Services.Dto.UnitTenant;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SearchUnitTenantsCommandConsumer : InMemoryConsumerBase<SearchUnitTenantsCommand, IList<UnitTenantReadDto>>
{
    private readonly IUserService _userService;

    public SearchUnitTenantsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<UnitTenantReadDto>> Consume(
        SearchUnitTenantsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var q = (command.Query ?? string.Empty).Trim().ToLower();

        var query = session.Query<UnitTenant>()
            .Where(t => t.Tenant.Id == command.TenantId && !t.IsDeleted);

        if (!string.IsNullOrEmpty(q))
            query = query.Where(t =>
                (t.FirstName != null && t.FirstName.ToLower().Contains(q)) ||
                (t.LastName  != null && t.LastName.ToLower().Contains(q))  ||
                (t.Email     != null && t.Email.ToLower().Contains(q)));

        var results = await query
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Take(30)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.Select(t => t.ToReadDto()).ToList();
    }
}
