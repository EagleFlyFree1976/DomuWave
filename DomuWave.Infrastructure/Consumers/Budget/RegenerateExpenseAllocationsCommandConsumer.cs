using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class RegenerateExpenseAllocationsCommandConsumer
    : InMemoryConsumerBase<RegenerateExpenseAllocationsCommand, RegenerateExpenseAllocationsResult>
{
    private readonly IUserService _userService;

    public RegenerateExpenseAllocationsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<RegenerateExpenseAllocationsResult> Consume(
        RegenerateExpenseAllocationsCommand command,
        IMediationContext                   mediationContext,
        CancellationToken                   cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await session.Query<Budget>()
            .Where(b => b.Id == command.BudgetId && !b.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (budget == null)
            throw new NotFoundException("Budget non trovato.");

        if (budget.Type != BudgetType.Consuntivo)
            throw new ValidatorException("La rigenerazione delle ripartizioni è disponibile solo per i budget consuntivi.");

        var condominiumId = budget.Condominium.Id;
        var fiscalYearId  = budget.FiscalYear.Id;

        var result = new RegenerateExpenseAllocationsResult
        {
            BudgetId      = command.BudgetId,
            CondominiumId = condominiumId,
            FiscalYearId  = fiscalYearId,
        };

        // NOTA: in passato qui si creava retroattivamente una Expense "riepilogo" per le
        // ConsumptionCharge approvate senza Expense collegata. Questo DUPLICAVA il costo:
        // le bollette reali sono già registrate sul conto del tipo consumo e vengono
        // ripartite per consumo (modello "solo bollette reali", coerente con
        // ApproveConsumptionCharge che NON crea riepiloghi). Il blocco è stato rimosso.

        // ── Spese dell'esercizio ──────────────────────────────────────────────
        var expenses = await session.Query<Expense>()
            .Where(e => e.Condominium.Id == condominiumId
                     && e.FiscalYear.Id  == fiscalYearId
                     && !e.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        result.ExpensesCount = expenses.Count;

        foreach (var expense in expenses)
        {
            await ExpenseAllocationHelper
                .RegenerateAllocationsAsync(session, expense, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }

        result.ProcessedCount = result.ExpensesCount;
        return result;
    }
}
