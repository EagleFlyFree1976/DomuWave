using CPQ.Core.Consumers;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class MarkExpenseAsUnpaidCommandConsumer : InMemoryConsumerBase<MarkExpenseAsUnpaidCommand, bool>
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService    _userService;
    private readonly IMediator       _mediator;

    public MarkExpenseAsUnpaidCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExpenseService expenseService,
        IUserService userService,
        IMediator mediator) : base(sessionFactoryProvider)
    {
        _expenseService = expenseService;
        _userService    = userService;
        _mediator       = mediator;
    }

    protected override async Task<bool> Consume(
        MarkExpenseAsUnpaidCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var expenseInfo = await session.Query<Expense>()
            .Where(e => e.Id == command.ExpenseId && !e.IsDeleted)
            .Select(e => new { CondominiumId = e.Condominium.Id, FiscalYearId = (int?)e.FiscalYear.Id })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = await _expenseService
            .MarkAsUnpaidAsync(command.ExpenseId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (expenseInfo?.FiscalYearId != null)
        {
            var budget = await session.Query<Budget>()
                .Where(b => b.Condominium.Id == expenseInfo.CondominiumId
                         && b.FiscalYear.Id == expenseInfo.FiscalYearId.Value
                         && b.Type == BudgetType.Consuntivo
                         && b.Status.Id != BudgetStatus.Closed
                         && !b.IsDeleted)
                .Select(b => new { b.Id })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (budget != null)
                await _mediator.GetResponse(new RecalculateBudgetItemsCommand(command.CurrentUserId, budget.Id), cancellationToken)
                    .ConfigureAwait(false);
        }

        return result;
    }
}
