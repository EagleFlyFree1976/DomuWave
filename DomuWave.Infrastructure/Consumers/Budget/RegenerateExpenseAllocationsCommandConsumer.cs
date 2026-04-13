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

        // ── Diagnostica: tutte le ConsumptionCharge dell'esercizio ───────────
        var allCharges = await session.Query<ConsumptionCharge>()
            .Where(c => c.FiscalYear.Id == fiscalYearId && !c.IsDeleted)
            .Select(c => new
            {
                c.Id,
                StatusId      = c.Status.Id,
                BudgetId      = c.Budget.Id,
                HasExpense    = c.Expense != null,
                ExpenseId     = c.Expense != null ? (long?)c.Expense.Id : null,
                TypeName      = c.ConsumptionType.Name,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        result.DiagCharges = allCharges.Select(c =>
            $"Charge#{c.Id} status={c.StatusId} budgetId={c.BudgetId} hasExpense={c.HasExpense} expenseId={c.ExpenseId} type={c.TypeName}"
        ).ToList();

        // ── 1. ConsumptionCharge approvate senza spesa — filtro per esercizio ─
        // Nota: il Budget su ConsumptionCharge è il PREVENTIVO, non il consuntivo.
        // Filtriamo solo per esercizio fiscale e stato approvato.
        var chargesWithoutExpense = await session.Query<ConsumptionCharge>()
            .Where(c => c.FiscalYear.Id == fiscalYearId
                     && c.Status.Id     == ConsumptionChargeStatus.Approved
                     && c.Expense       == null
                     && !c.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        result.ChargesWithoutExpenseCount = chargesWithoutExpense.Count;

        foreach (var charge in chargesWithoutExpense)
        {
            var items = await session.Query<ConsumptionChargeItem>()
                .Where(ci => ci.Charge.Id == charge.Id && !ci.IsDeleted && ci.Amount > 0)
                .Select(ci => new { UnitId = ci.Unit.Id, ci.Amount, ci.Percentage })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!items.Any())
            {
                result.DiagMessages.Add($"Charge#{charge.Id}: skip — nessun item con Amount > 0");
                continue;
            }

            var millesimalTable = await session.Query<MillesimalTable>()
                .Where(m => m.Condominium.Id == condominiumId && m.IsEnabled && !m.IsDeleted)
                .OrderBy(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (millesimalTable == null)
            {
                result.DiagMessages.Add($"Charge#{charge.Id}: skip — nessuna tabella millesimale attiva");
                continue;
            }

            var condominium   = session.Load<Models.Condominium>(condominiumId);
            var fiscalYear    = session.Load<FiscalYear>(fiscalYearId);
            var expenseType   = session.Load<ExpenseType>(ExpenseType.Altro);
            var paymentStatus = session.Load<ExpensePaymentStatus>(ExpensePaymentStatus.DaPagare);
            var chargeType    = session.Load<ChargeabilityType>(ChargeabilityType.Owner);

            var expense = new Expense
            {
                Condominium       = condominium,
                Tenant            = budget.Tenant,
                Account           = charge.ConsumptionType?.Account,
                FiscalYear        = fiscalYear,
                MillesimalTable   = millesimalTable,
                ExpenseType       = expenseType,
                PaymentStatus     = paymentStatus,
                ChargeabilityType = chargeType,
                Name              = $"Consumi {charge.ConsumptionType?.Name} – {charge.FiscalYear?.Code}",
                DocumentDate      = charge.CreationDate,
                RegistrationDate  = charge.CreationDate,
                GrossAmount       = charge.TotalAmount,
                VatAmount         = 0m,
                NetAmount         = charge.TotalAmount,
                Notes             = $"[Ripartizione consumi #{charge.Id}] generata automaticamente (retroattiva)",
            };
            expense.Trace(currentUser);
            await session.SaveAsync(expense, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

            charge.Expense = expense;
            charge.Trace(currentUser);
            await session.SaveOrUpdateAsync(charge, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

            await ExpenseAllocationHelper
                .RegenerateAllocationsAsync(session, expense, currentUser, cancellationToken)
                .ConfigureAwait(false);

            result.DiagMessages.Add($"Charge#{charge.Id}: creata Expense#{expense.Id} con {items.Count} allocazioni");
            result.CreatedExpensesCount++;
        }

        // ── 2. Spese dell'esercizio ───────────────────────────────────────────
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

        result.ProcessedCount = result.CreatedExpensesCount + result.ExpensesCount;
        return result;
    }
}
