using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class RegenerateExpenseAllocationsCommandConsumer
    : InMemoryConsumerBase<RegenerateExpenseAllocationsCommand, int>
{
    private readonly IUserService _userService;

    public RegenerateExpenseAllocationsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<int> Consume(
        RegenerateExpenseAllocationsCommand command,
        IMediationContext                   mediationContext,
        CancellationToken                   cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        // Verifica budget
        var budgetRow = await session.Query<Budget>()
            .Where(b => b.Id == command.BudgetId && !b.IsDeleted)
            .Select(b => new
            {
                b.Type,
                b.Status,
                CondominiumId = b.Condominium.Id,
                FiscalYearId  = b.FiscalYear.Id,
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (budgetRow == null)
            throw new NotFoundException("Budget non trovato.");

        if (budgetRow.Type != BudgetType.Consuntivo)
            throw new ValidatorException("La rigenerazione delle ripartizioni è disponibile solo per i budget consuntivi.");

        // Carica tutte le spese dell'esercizio (con MillesimalTable)
        var expenses = await session.Query<Expense>()
            .Where(e => e.Condominium.Id == budgetRow.CondominiumId
                     && e.FiscalYear.Id  == budgetRow.FiscalYearId
                     && !e.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        int count = 0;
        foreach (var expense in expenses)
        {
            if (expense.MillesimalTable == null)
                continue;

            await ExpenseAllocationHelper
                .RegenerateAllocationsAsync(session, expense, currentUser, cancellationToken)
                .ConfigureAwait(false);

            count++;
        }

        return count;
    }
}
