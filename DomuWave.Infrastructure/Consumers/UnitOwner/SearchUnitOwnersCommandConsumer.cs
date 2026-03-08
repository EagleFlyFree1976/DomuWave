using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SearchUnitOwnersCommandConsumer : InMemoryConsumerBase<SearchUnitOwnersCommand, IList<UnitOwnerReadDto>>
{
    private readonly IUserService _userService;

    public SearchUnitOwnersCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<IList<UnitOwnerReadDto>> Consume(
        SearchUnitOwnersCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var q = (command.Query ?? string.Empty).Trim().ToLower();

        var query = session.Query<UnitOwner>()
            .Where(o => o.Tenant.Id == command.TenantId && !o.IsDeleted);

        if (!string.IsNullOrEmpty(q))
            query = query.Where(o =>
                (o.FirstName != null && o.FirstName.ToLower().Contains(q)) ||
                (o.LastName  != null && o.LastName.ToLower().Contains(q))  ||
                (o.Email     != null && o.Email.ToLower().Contains(q)));

        var results = await query
            .OrderBy(o => o.LastName)
            .ThenBy(o => o.FirstName)
            .Take(30)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results.Select(o => o.ToReadDto()).ToList();
    }
}
