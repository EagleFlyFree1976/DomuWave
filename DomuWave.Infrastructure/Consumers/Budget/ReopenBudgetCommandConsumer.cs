using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class ReopenBudgetCommandConsumer
    : InMemoryConsumerBase<ReopenBudgetCommand, bool>
{
    private readonly IBudgetService _budgetService;
    private readonly IUserService   _userService;

    public ReopenBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService budgetService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetService = budgetService;
        _userService   = userService;
    }

    protected override async Task<bool> Consume(
        ReopenBudgetCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await session.Query<Budget>()
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (budget == null)
            throw new NotFoundException("Budget non trovato.");

        if (budget.Status?.Id != BudgetStatus.Approved && budget.Status?.Id != BudgetStatus.PendingApproval)
            throw new ValidatorException("Solo i budget in stato 'In approvazione' o 'Approvato' possono essere riaperti in bozza.");

        if (budget.Status?.Id == BudgetStatus.Approved && !currentUser.IsSystemUser)
            throw new ValidatorException("Solo il SuperAdmin può riaprire un budget già approvato.");

        budget.Status       = session.Load<BudgetStatus>(BudgetStatus.Draft);
        budget.ApprovalDate = null;
        budget.Trace(currentUser);

        await session.SaveOrUpdateAsync(budget, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
