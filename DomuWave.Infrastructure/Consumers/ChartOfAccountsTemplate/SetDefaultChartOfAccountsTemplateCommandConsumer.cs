using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.ChartOfAccountsTemplate;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class SetDefaultChartOfAccountsTemplateCommandConsumer
    : InMemoryConsumerBase<SetDefaultChartOfAccountsTemplateCommand, bool>
{
    private readonly IUserService _userService;

    public SetDefaultChartOfAccountsTemplateCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
        => _userService = userService;

    protected override async Task<bool> Consume(
        SetDefaultChartOfAccountsTemplateCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(command.CurrentUserId, cancellationToken).ConfigureAwait(false);

        var target = await session.Query<ChartOfAccountsTemplate>()
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken).ConfigureAwait(false);
        if (target == null)
            throw new NotFoundException("Template non trovato.");

        // Remove IsDefault from all other templates of the same tenant
        var others = await session.Query<ChartOfAccountsTemplate>()
            .Where(t => t.Tenant.Id == target.Tenant.Id && t.Id != command.Id && t.IsDefault && !t.IsDeleted)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var other in others)
        {
            other.IsDefault = false;
            other.Trace(currentUser);
            await session.UpdateAsync(other, cancellationToken).ConfigureAwait(false);
        }

        target.IsDefault = true;
        target.Trace(currentUser);
        await session.UpdateAsync(target, cancellationToken).ConfigureAwait(false);

        return true;
    }
}
