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

public class ApproveBudgetCommandConsumer
    : InMemoryConsumerBase<ApproveBudgetCommand, bool>
{
    private readonly IBudgetService _budgetService;
    private readonly IUserService   _userService;

    public ApproveBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService budgetService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetService = budgetService;
        _userService   = userService;
    }

    protected override async Task<bool> Consume(
        ApproveBudgetCommand command,
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

        if (budget.Status?.Id != BudgetStatus.Draft)
            throw new ValidatorException("Solo i budget in stato Bozza possono essere approvati.");

        var tipoLabel = budget.Type == BudgetType.Preventivo ? "preventivo" : "consuntivo";
        var conflicting = await session.Query<Budget>()
            .AnyAsync(x => x.Id             != command.Id
                        && x.Condominium.Id == budget.Condominium.Id
                        && x.FiscalYear.Id  == budget.FiscalYear.Id
                        && x.Type           == budget.Type
                        && (x.Status.Id == BudgetStatus.Approved || x.Status.Id == BudgetStatus.Closed)
                        && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (conflicting)
            throw new ValidatorException(
                $"Esiste già un budget {tipoLabel} approvato o chiuso per questo esercizio e condominio. " +
                "Non è possibile approvarne un altro.");

        // Approva il budget
        var approved = await _budgetService
            .ApproveBudgetAsync(command.Id, command.CurrentUserId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        if (!approved)
            return false;

        if (budget.Type == BudgetType.Preventivo)
        {
            // Genera rate di pagamento e quote per unità
            await GenerateInstallmentsAndFees(budget, command, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            // Consuntivo: salva la ripartizione per unità (saldi), senza rate
            await GenerateConsuntivoDistribution(budget, currentUser, cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
    }

    private async Task GenerateInstallmentsAndFees(
        Budget budget,
        ApproveBudgetCommand command,
        object currentUser,
        CancellationToken cancellationToken)
    {
        var n            = command.NumberOfInstallments > 0 ? command.NumberOfInstallments : 4;
        var firstDueDate = command.FirstDueDate == default ? DateTime.Today : command.FirstDueDate;

        // Verifica che non esistano già rate per questo budget
        var existingCount = await session.Query<CondominiumInstallment>()
            .CountAsync(x => x.Budget.Id == budget.Id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (existingCount > 0)
            return; // già generate, skip

        // Carica la tabella millesimale attiva del condominio
        var millesimalTable = await session.Query<MillesimalTable>()
            .FirstOrDefaultAsync(x => x.Condominium.Id == budget.Condominium.Id
                                   && x.IsActive && !x.IsDraft && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        // Carica le voci millesimali (quote per unità)
        var unitMillesimals = millesimalTable != null
            ? await session.Query<UnitMillesimal>()
                .Where(x => x.MillesimalTable.Id == millesimalTable.Id && !x.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false)
            : new List<UnitMillesimal>();

        var totalMillesimal = millesimalTable?.TotalMillesimal ?? 0m;
        var amountPerInstallment = n > 0 ? Math.Round(budget.TotalIncome / n, 2) : 0m;
        var user = currentUser as CPQ.Core.Memberships.IUser;
        var openStatus = await session.Query<CondominiumInstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == CondominiumInstallmentStatus .Open , cancellationToken)
            .ConfigureAwait(false);
        for (int i = 1; i <= n; i++)
        {
            // Scadenza: ogni rata a distanza di 30 giorni (approssimazione mensile)
            var dueDate = firstDueDate.AddMonths(i - 1);

            var installment = new CondominiumInstallment
            {
                Condominium       = budget.Condominium,
                Budget            = budget,
                FiscalYear        = budget.FiscalYear,
                Tenant            = budget.Tenant,
                InstallmentNumber = i,
                DueDate           = dueDate,
                TotalAmount       = amountPerInstallment,
                Status            = openStatus,
                Notes             = $"Rata {i}/{n} – {budget.FiscalYear?.Code ?? dueDate.Year.ToString()}",
            };

            if (user != null) installment.Trace(user);

            await session.SaveOrUpdateAsync(installment, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

            // Genera una quota per ogni unità della tabella millesimale
            foreach (var um in unitMillesimals)
            {
                var share = totalMillesimal > 0
                    ? Math.Round(amountPerInstallment * um.Millesimal / totalMillesimal, 2)
                    : 0m;

                // Recupera il proprietario dell'unità (UnitOwner attivo)
                var owner = await session.Query<UnitOwner>()
                    .FirstOrDefaultAsync(x => x.Unit.Id == um.Unit.Id && !x.IsDeleted, cancellationToken)
                    .ConfigureAwait(false);

                var fee = new CondominiumFee
                {
                    Installment   = installment,
                    Unit          = um.Unit,
                    UserId        = owner?.UserId ?? 0,
                    Tenant        = budget.Tenant,
                    AmountDue     = share,
                    AmountPaid    = 0m,
                    Balance       = share,
                    PaymentStatus = "ToPay",
                };

                if (user != null) fee.Trace(user);

                await session.SaveOrUpdateAsync(fee, cancellationToken).ConfigureAwait(false);
            }

            await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    // ── Consuntivo: ripartizione saldi per unità (senza rate) ────────────────

    private async Task GenerateConsuntivoDistribution(
        Budget budget,
        object currentUser,
        CancellationToken ct)
    {
        // Ricalcola TotalExpenses dalle voci del budget
        var totalExpenses = await session.Query<BudgetItem>()
            .Where(i => i.Budget.Id == budget.Id && !i.IsDeleted)
            .SumAsync(i => i.Amount, ct)
            .ConfigureAwait(false);

        if (totalExpenses <= 0) return;

        // Tabella millesimale attiva del condominio
        var millesimalTable = await session.Query<MillesimalTable>()
            .FirstOrDefaultAsync(x => x.Condominium.Id == budget.Condominium.Id
                                   && x.IsActive && !x.IsDraft && !x.IsDeleted, ct)
            .ConfigureAwait(false);

        if (millesimalTable == null) return;

        var unitMillesimals = await session.Query<UnitMillesimal>()
            .Where(x => x.MillesimalTable.Id == millesimalTable.Id && !x.IsDeleted)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (!unitMillesimals.Any()) return;

        var totalMillesimal = millesimalTable.TotalMillesimal;
        var user = currentUser as CPQ.Core.Memberships.IUser;

        // Recupera i saldi già pagati per unità (da CondominiumFee del Preventivo approvato)
        // per calcolare il saldo residuo per ogni unità
        var paidByUnit = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Budget.Condominium.Id == budget.Condominium.Id
                     && f.Installment.Budget.FiscalYear.Id  == budget.FiscalYear.Id
                     && f.Installment.Budget.Type           == BudgetType.Preventivo
                     && !f.IsDeleted)
            .GroupBy(f => f.Unit.Id)
            .Select(g => new { UnitId = g.Key, TotalPaid = g.Sum(f => f.AmountPaid) })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var paidMap = paidByUnit.ToDictionary(x => x.UnitId, x => x.TotalPaid);

        // Salva la ripartizione consuntiva su UnitOpeningBalance (saldo)
        // Per ogni unità: quota consuntiva - già pagato = saldo residuo/credito
        foreach (var um in unitMillesimals)
        {
            var quotaConsuntiva = totalMillesimal > 0
                ? Math.Round(totalExpenses * um.Millesimal / totalMillesimal, 2)
                : 0m;

            var alreadyPaid = paidMap.TryGetValue(um.Unit.Id, out var p) ? p : 0m;
            var saldo = quotaConsuntiva - alreadyPaid;  // positivo = deve ancora pagare, negativo = credito

            // Aggiorna o crea il record UnitOpeningBalance con il saldo consuntivo
            var uob = await session.Query<UnitOpeningBalance>()
                .FirstOrDefaultAsync(x => x.Unit.Id       == um.Unit.Id
                                       && x.FiscalYear.Id == budget.FiscalYear.Id
                                       && !x.IsDeleted, ct)
                .ConfigureAwait(false);

            if (uob != null)
            {
                uob.TotalMovements  = quotaConsuntiva;
                uob.ClosingBalance  = saldo;
                if (user != null) uob.Trace(user);
                await session.SaveOrUpdateAsync(uob, ct).ConfigureAwait(false);
            }
        }

        await session.FlushAsync(ct).ConfigureAwait(false);
    }
}
