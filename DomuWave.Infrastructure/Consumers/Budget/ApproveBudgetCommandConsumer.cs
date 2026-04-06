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

        // Verifica che il piano dei conti abbia almeno un conto per ogni tipo
        var hasEntrata     = await session.Query<ChartOfAccounts>()
            .AnyAsync(a => a.Condominium.Id == budget.Condominium.Id && a.Type == ChartOfAccountsType.Entrata && a.IsActive && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        var hasUscita      = await session.Query<ChartOfAccounts>()
            .AnyAsync(a => a.Condominium.Id == budget.Condominium.Id && a.Type == ChartOfAccountsType.Uscita && a.IsActive && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);
        var hasPatrimoniale = await session.Query<ChartOfAccounts>()
            .AnyAsync(a => a.Condominium.Id == budget.Condominium.Id && a.Type == ChartOfAccountsType.Patrimoniale && a.IsActive && !a.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!hasEntrata || !hasUscita || !hasPatrimoniale)
        {
            var missing = new List<string>();
            if (!hasEntrata)      missing.Add("Entrata");
            if (!hasUscita)       missing.Add("Uscita");
            if (!hasPatrimoniale) missing.Add("Patrimoniale");
            throw new ValidatorException(
                $"Il piano dei conti non è completo. Mancano conti di tipo: {string.Join(", ", missing)}. " +
                "Configura il piano dei conti prima di approvare il budget.");
        }

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

        // Carica la tabella millesimale abilitata del condominio (preferisce la DEF se presente)
        var millesimalTable = await session.Query<MillesimalTable>()
            .Where(x => x.Condominium.Id == budget.Condominium.Id && x.IsEnabled && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        // Carica le voci millesimali (quote per unità)
        var unitMillesimals = millesimalTable != null
            ? await session.Query<UnitMillesimal>()
                .Where(x => x.MillesimalTable.Id == millesimalTable.Id && !x.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false)
            : new List<UnitMillesimal>();

        // Ricalcola TotalIncome live dalle voci budget (somma tutto: include Entrata + Patrimoniale)
        // Nota: per il preventivo il totale da ripartire è la somma di tutte le voci (non solo le Entrate)
        var totalIncome = await session.Query<BudgetItem>()
            .Where(x => x.Budget.Id == budget.Id && !x.IsDeleted)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var totalMillesimal = unitMillesimals.Count > 0
            ? unitMillesimals.Sum(x => x.Millesimal)
            : (millesimalTable?.TotalMillesimal ?? 0m);
        var user = currentUser as CPQ.Core.Memberships.IUser;
        var openStatus = await session.Query<CondominiumInstallmentStatus>()
            .FirstOrDefaultAsync(x => x.Id == CondominiumInstallmentStatus.Open, cancellationToken)
            .ConfigureAwait(false);

        // Pre-calcola gli importi delle rate distribuendo il residuo sull'ultima
        var installmentAmounts = new decimal[n];
        var baseAmount = n > 0 ? Math.Round(totalIncome / n, 2) : 0m;
        var allocated = 0m;
        for (int i = 0; i < n - 1; i++) { installmentAmounts[i] = baseAmount; allocated += baseAmount; }
        installmentAmounts[n - 1] = totalIncome - allocated;

        for (int i = 1; i <= n; i++)
        {
            var dueDate    = firstDueDate.AddMonths(i - 1);
            var instAmount = installmentAmounts[i - 1];

            var installment = new CondominiumInstallment
            {
                Condominium       = budget.Condominium,
                Budget            = budget,
                FiscalYear        = budget.FiscalYear,
                Tenant            = budget.Tenant,
                InstallmentNumber = i,
                DueDate           = dueDate,
                TotalAmount       = instAmount,
                Status            = openStatus,
                Notes             = $"Rata {i}/{n} – {budget.FiscalYear?.Code ?? dueDate.Year.ToString()}",
            };

            if (user != null) installment.Trace(user);

            await session.SaveOrUpdateAsync(installment, cancellationToken).ConfigureAwait(false);
            await session.FlushAsync(cancellationToken).ConfigureAwait(false);

            // Distribuisce le quote tra le unità, residuo sull'ultima unità
            var feeAllocated = 0m;
            for (int j = 0; j < unitMillesimals.Count; j++)
            {
                var um         = unitMillesimals[j];
                var isLastUnit = j == unitMillesimals.Count - 1;
                decimal share;
                if (isLastUnit)
                    share = instAmount - feeAllocated;
                else
                {
                    share = totalMillesimal > 0
                        ? Math.Round(instAmount * um.Millesimal / totalMillesimal, 2)
                        : 0m;
                    feeAllocated += share;
                }

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

    // ── Consuntivo: scrive QuotaConsuntiva e SaldoConguaglio su UnitOpeningBalance ──
    // NON sovrascrive TotalMovements né ClosingBalance: quelli vengono calcolati
    // definitivamente da CloseFiscalYearCommandConsumer.ComputeUnitClosingBalances.

    private async Task GenerateConsuntivoDistribution(
        Budget budget,
        object currentUser,
        CancellationToken ct)
    {
        // Ricalcola TotalExpenses dalle voci del budget consuntivo
        var totalExpenses = await session.Query<BudgetItem>()
            .Where(i => i.Budget.Id == budget.Id && !i.IsDeleted)
            .SumAsync(i => i.Amount, ct)
            .ConfigureAwait(false);

        if (totalExpenses <= 0) return;

        // Tabella millesimale abilitata del condominio
        var millesimalTable = await session.Query<MillesimalTable>()
            .Where(x => x.Condominium.Id == budget.Condominium.Id && x.IsEnabled && !x.IsDeleted)
            .OrderBy(x => x.Code)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (millesimalTable == null) return;

        var unitMillesimals = await session.Query<UnitMillesimal>()
            .Where(x => x.MillesimalTable.Id == millesimalTable.Id && !x.IsDeleted)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (!unitMillesimals.Any()) return;

        var totalMillesimal = unitMillesimals.Any()
            ? unitMillesimals.Sum(x => x.Millesimal)
            : millesimalTable.TotalMillesimal;

        var user = currentUser as CPQ.Core.Memberships.IUser;

        // Rate addebitate per unità dal Preventivo dell'esercizio (fonte: CondominiumFee.AmountDue)
        var rateAddebitatByUnit = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Budget.Condominium.Id == budget.Condominium.Id
                     && f.Installment.Budget.FiscalYear.Id  == budget.FiscalYear.Id
                     && f.Installment.Budget.Type           == BudgetType.Preventivo
                     && !f.IsDeleted)
            .GroupBy(f => f.Unit.Id)
            .Select(g => new { UnitId = g.Key, TotalDue = g.Sum(f => f.AmountDue) })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var rateMap = rateAddebitatByUnit.ToDictionary(x => x.UnitId, x => x.TotalDue);

        foreach (var um in unitMillesimals)
        {
            var quotaConsuntiva = totalMillesimal > 0
                ? Math.Round(totalExpenses * um.Millesimal / totalMillesimal, 2)
                : 0m;

            // SaldoConguaglio = quota reale consuntiva - quanto già addebitato con le rate del preventivo
            // Positivo = il condòmino deve un conguaglio a debito
            // Negativo = il condominio deve restituire un credito al condòmino
            var rateAddebitate  = rateMap.TryGetValue(um.Unit.Id, out var r) ? r : 0m;
            var saldoConguaglio = quotaConsuntiva - rateAddebitate;

            var uob = await session.Query<UnitOpeningBalance>()
                .FirstOrDefaultAsync(x => x.Unit.Id       == um.Unit.Id
                                       && x.FiscalYear.Id == budget.FiscalYear.Id
                                       && !x.IsDeleted, ct)
                .ConfigureAwait(false);

            if (uob != null)
            {
                // Aggiorna solo i campi di pertinenza del Consuntivo
                uob.QuotaConsuntiva = quotaConsuntiva;
                uob.SaldoConguaglio = saldoConguaglio;
                // TotalMovements e ClosingBalance restano invariati qui:
                // verranno ricalcolati definitivamente alla chiusura dell'esercizio.
                if (user != null) uob.Trace(user);
                await session.SaveOrUpdateAsync(uob, ct).ConfigureAwait(false);
            }
            else
            {
                // Crea il record se non esiste ancora (es. Consuntivo approvato prima della chiusura
                // su un'unità che non ha ancora rate)
                var unit = um.Unit;
                var newUob = new UnitOpeningBalance
                {
                    Unit            = unit,
                    FiscalYear      = budget.FiscalYear,
                    Tenant          = budget.Tenant,
                    OpeningBalance  = 0m,
                    RateAddebitate  = rateAddebitate,
                    RateIncassate   = 0m,
                    QuotaConsuntiva = quotaConsuntiva,
                    SaldoConguaglio = saldoConguaglio,
                    TotalMovements  = 0m,
                    ClosingBalance  = 0m,
                };
                if (user != null) newUob.Trace(user);
                await session.SaveAsync(newUob, ct).ConfigureAwait(false);
            }
        }

        await session.FlushAsync(ct).ConfigureAwait(false);
    }
}
