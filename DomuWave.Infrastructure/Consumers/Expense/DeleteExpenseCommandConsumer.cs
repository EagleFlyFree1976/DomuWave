using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Interfaces;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteExpenseCommandConsumer : InMemoryConsumerBase<DeleteExpenseCommand, bool>
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService    _userService;
    private readonly IMediator       _mediator;

    public DeleteExpenseCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExpenseService         expenseService,
        IUserService            userService,
        IMediator               mediator) : base(sessionFactoryProvider)
    {
        _expenseService = expenseService;
        _userService    = userService;
        _mediator       = mediator;
    }

    protected override async Task<bool> Consume(
        DeleteExpenseCommand command,
        IMediationContext    mediationContext,
        CancellationToken    cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Read condominium/fiscal year before deleting so we can sync the consuntivo afterward
        var expenseRow = await session.Query<Expense>()
            .Where(x => x.Id == command.ExpenseId && !x.IsDeleted)
            .Select(x => new { CondominiumId = x.Condominium.Id, FiscalYearId = x.FiscalYear.Id })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var deleted = await _expenseService
            .DeleteAsync(command.ExpenseId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (deleted && expenseRow != null)
        {
            var consuntivo = await session.Query<Budget>()
                .Where(b => b.Condominium.Id == expenseRow.CondominiumId
                         && b.FiscalYear.Id  == expenseRow.FiscalYearId
                         && b.Type           == BudgetType.Consuntivo
                         && b.Status.Id      != BudgetStatus.Closed
                         && !b.IsDeleted)
                .Select(b => new { b.Id })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (consuntivo != null)
                await _mediator
                    .GetResponse(new RecalculateBudgetItemsCommand(command.CurrentUserId, consuntivo.Id), cancellationToken)
                    .ConfigureAwait(false);
        }

        return deleted;
    }
}
