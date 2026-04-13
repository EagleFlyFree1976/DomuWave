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

        // ── 1. Ripartizioni consumi approvate senza spesa collegata ─────────
        // Queste esistono quando la ripartizione è stata approvata prima che
        // il tipo consumo avesse un conto del piano dei conti configurato.
        // Le gestiamo creando le ExpenseAllocation direttamente dai ConsumptionChargeItem.
        var chargesWithoutExpense = await session.Query<ConsumptionCharge>()
            .Where(c => c.FiscalYear.Id  == fiscalYearId
                     && c.Budget.Id      == command.BudgetId
                     && c.Status.Id      == ConsumptionChargeStatus.Approved
                     && c.Expense        == null
                     && !c.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        int count = 0;

        foreach (var charge in chargesWithoutExpense)
        {
            var items = await session.Query<ConsumptionChargeItem>()
                .Where(ci => ci.Charge.Id == charge.Id && !ci.IsDeleted && ci.Amount > 0)
                .Select(ci => new { UnitId = ci.Unit.Id, ci.Amount, ci.Percentage })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!items.Any())
                continue;

            // Crea una spesa fittizia per questa ripartizione, anche senza Account,
            // in modo da poter agganciare le ExpenseAllocation al dettaglio consuntivo.
            var condominium = session.Load<Models.Condominium>(condominiumId);
            var fiscalYear  = session.Load<FiscalYear>(fiscalYearId);

            // Tabella millesimale di default — obbligatoria su Expense ma non usata
            // per il calcolo (quello viene dai ConsumptionChargeItem)
            var millesimalTable = await session.Query<MillesimalTable>()
                .Where(m => m.Condominium.Id == condominiumId && m.IsEnabled && !m.IsDeleted)
                .OrderBy(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (millesimalTable == null)
                continue; // non possiamo creare la spesa senza tabella millesimale

            var expenseType       = session.Load<ExpenseType>(ExpenseType.Altro);
            var paymentStatus     = session.Load<ExpensePaymentStatus>(ExpensePaymentStatus.DaPagare);
            var chargeabilityType = session.Load<ChargeabilityType>(ChargeabilityType.Owner);

            var expense = new Expense
            {
                Condominium       = condominium,
                Tenant            = budget.Tenant,
                Account           = charge.ConsumptionType?.Account,
                FiscalYear        = fiscalYear,
                MillesimalTable   = millesimalTable,
                ExpenseType       = expenseType,
                PaymentStatus     = paymentStatus,
                ChargeabilityType = chargeabilityType,
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

            // Collega la spesa alla ripartizione
            charge.Expense = expense;
            charge.Trace(currentUser);
            await session.SaveOrUpdateAsync(charge, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

            // Genera le ExpenseAllocation dai ConsumptionChargeItem
            await ExpenseAllocationHelper
                .RegenerateAllocationsAsync(session, expense, currentUser, cancellationToken)
                .ConfigureAwait(false);

            count++;
        }

        // ── 2. Spese dell'esercizio (incluse quelle appena create) ───────────
        var expenses = await session.Query<Expense>()
            .Where(e => e.Condominium.Id == condominiumId
                     && e.FiscalYear.Id  == fiscalYearId
                     && !e.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var expense in expenses)
        {
            await ExpenseAllocationHelper
                .RegenerateAllocationsAsync(session, expense, currentUser, cancellationToken)
                .ConfigureAwait(false);

            count++;
        }

        return count;
    }
}
