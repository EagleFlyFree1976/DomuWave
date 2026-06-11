using DomuWave.Services.Interfaces.Extensions;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Models;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetConsuntivoDetailCommandConsumer
    : InMemoryConsumerBase<GetConsuntivoDetailCommand, ConsuntivoDetailDto>
{
    private readonly IUserService _userService;

    public GetConsuntivoDetailCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _userService = userService;
    }

    protected override async Task<ConsuntivoDetailDto> Consume(
        GetConsuntivoDetailCommand command,
        IMediationContext           mediationContext,
        CancellationToken          cancellationToken)
    {
        var budgetRow = await session.Query<Budget>()
            .Where(b => b.Id == command.BudgetId && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                b.Type,
                FiscalYearCode = b.FiscalYear.Code,
                CondominiumId  = b.Condominium.Id,
                FiscalYearId   = b.FiscalYear.Id,
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (budgetRow == null)
            throw new NotFoundException("Budget non trovato.");

        if (budgetRow.Type != BudgetType.Consuntivo)
            throw new ValidatorException("Il dettaglio ripartizione è disponibile solo per i budget consuntivi.");

        var condominiumId = budgetRow.CondominiumId;
        var fiscalYearId  = budgetRow.FiscalYearId;

        // ── 1. Spese dell'esercizio ──────────────────────────────────────────
        var expenses = await session.Query<Expense>()
            .Where(e => e.Condominium.Id == condominiumId
                     && e.FiscalYear.Id  == fiscalYearId
                     && !e.IsDeleted)
            .Select(e => new
            {
                ExpenseId       = e.Id,
                ExpenseName     = e.Name,
                // Costo a carico condòmini = totale fattura = GrossAmount (già al netto
                // della ritenuta) + ritenuta d'acconto. Coerente con bilancio e report.
                GrossAmount     = e.GrossAmount + e.WithholdingTax,
                DocumentDate    = e.DocumentDate,
                SupplierName    = e.Supplier != null ? e.Supplier.Name : null,
                AccountId       = e.Account.Id,
                AccountCode     = e.Account.Code,
                AccountName     = e.Account.Name,
                AccountLevel    = e.Account.Level,
                ParentAccountId = e.Account.ParentAccount != null ? (int?)e.Account.ParentAccount.Id : null,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // ── 2. Allocazioni per unità delle spese ────────────────────────────
        var expenseIds = expenses.Select(e => e.ExpenseId).ToList();

        var allocations = expenseIds.Any()
            ? (await session.Query<ExpenseAllocation>()
                .Where(a => expenseIds.Contains(a.Expense.Id) && !a.IsDeleted && a.AllocatedAmount != 0)
                .Select(a => new
                {
                    ExpenseId      = a.Expense.Id,
                    UnitId         = a.Unit.Id,
                    InternalNumber = a.Unit.InternalNumber,
                    DisplayName    = a.Unit.DisplayName,
                    a.Millesimal,
                    a.AllocatedAmount,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
                .Select(a => new
                {
                    a.ExpenseId,
                    a.UnitId,
                    UnitName = RealEstateUnitMappingExtensions.FormatUnitName(a.InternalNumber, a.DisplayName),
                    a.Millesimal,
                    a.AllocatedAmount,
                })
                .ToList()
            : [];

        var allocationsByExpense = allocations
            .GroupBy(a => a.ExpenseId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ── 2b. Conti a consumo + quote consumi autoritative (ConsumptionChargeItem) ─
        // Per i conti a consumo CON bollette l'importo per unità è quello memorizzato
        // sulla ripartizione consumi approvata (fonte autoritativa), non il
        // redistribuito dalle ExpenseAllocation delle bollette: così il pivot per unità
        // coincide al centesimo col Bilancio di ripartizione.
        // NB: ci limitiamo ai conti che hanno effettivamente bollette (Expense): le
        // ripartizioni "vecchio stile" senza bollette sono già gestite come voci
        // sintetiche nella sezione 3 (evita doppio conteggio).
        var accountsWithExpenses = expenses
            .Select(e => e.AccountId)
            .Distinct()
            .ToHashSet();

        var consumptionAccountIds = (await session.Query<ConsumptionType>()
                .Where(t => t.Condominium.Id == condominiumId && !t.IsDeleted
                         && t.Account != null)
                .Select(t => t.Account.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .Where(id => id > 0 && accountsWithExpenses.Contains(id))
            .ToHashSet();

        // Quote consumi approvate dell'esercizio, aggregate per (conto consumo, unità).
        var consumptionItemRows = (await session.Query<ConsumptionChargeItem>()
                .Where(ci => !ci.IsDeleted
                          && !ci.Charge.IsDeleted
                          && ci.Charge.Status.Id     == ConsumptionChargeStatus.Approved
                          && ci.Charge.FiscalYear.Id == fiscalYearId
                          && ci.Charge.ConsumptionType.Account != null)
                .Select(ci => new
                {
                    AccountId      = ci.Charge.ConsumptionType.Account.Id,
                    UnitId         = ci.Unit.Id,
                    InternalNumber = ci.Unit.InternalNumber,
                    DisplayName    = ci.Unit.DisplayName,
                    ci.Amount,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .Where(ci => consumptionAccountIds.Contains(ci.AccountId))
            .Select(ci => new
            {
                ci.AccountId,
                ci.UnitId,
                UnitName = RealEstateUnitMappingExtensions.FormatUnitName(ci.InternalNumber, ci.DisplayName),
                ci.Amount,
            })
            .ToList();

        // Aggregato per (conto, unità) — usato sia per i totali-conto sia per il pivot.
        var consumptionByAccountUnit = consumptionItemRows
            .GroupBy(x => (x.AccountId, x.UnitId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // ── 3. ConsumptionCharge approvate SENZA spese sul conto ─────────────
        // Voci sintetiche SOLO per le ripartizioni "vecchio stile" che non hanno
        // bollette (Expense) sul conto del tipo consumo: in tal caso le quote non
        // sono rappresentate da alcuna ExpenseAllocation e vanno aggiunte qui.
        // Quando invece esistono bollette sul conto (nuovo flusso), le loro quote
        // (ConsumptionChargeItem) sono già conteggiate tramite consumptionItemRows
        // → niente da aggiungere qui (altrimenti si conterebbe due volte).
        // (accountsWithExpenses è già calcolato nella sezione 2b.)
        var chargesAll = await session.Query<ConsumptionCharge>()
            .Where(c => c.FiscalYear.Id == fiscalYearId
                     && c.Status.Id     == ConsumptionChargeStatus.Approved
                     && c.Expense       == null
                     && !c.IsDeleted)
            .Select(c => new
            {
                ChargeId        = c.Id,
                ChargeName      = "Consumi " + c.ConsumptionType.Name,
                TotalAmount     = c.TotalAmount,
                DocumentDate    = c.CreationDate,
                TypeName        = c.ConsumptionType.Name,
                AccountId       = c.ConsumptionType.Account != null ? (int?)c.ConsumptionType.Account.Id : null,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Escludi le charge il cui conto ha già bollette (già coperte dalle ExpenseAllocation)
        var chargesWithoutExpense = chargesAll
            .Where(c => !(c.AccountId.HasValue && accountsWithExpenses.Contains(c.AccountId.Value)))
            .ToList();

        // Recupera gli item per queste charge
        var chargeIds = chargesWithoutExpense.Select(c => c.ChargeId).ToList();
        var chargeItems = chargeIds.Any()
            ? (await session.Query<ConsumptionChargeItem>()
                .Where(ci => chargeIds.Contains(ci.Charge.Id) && !ci.IsDeleted && ci.Amount > 0)
                .Select(ci => new
                {
                    ChargeId       = ci.Charge.Id,
                    UnitId         = ci.Unit.Id,
                    InternalNumber = ci.Unit.InternalNumber,
                    DisplayName    = ci.Unit.DisplayName,
                    ci.Amount,
                    ci.Percentage,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
                .Select(ci => new
                {
                    ci.ChargeId,
                    ci.UnitId,
                    UnitName = RealEstateUnitMappingExtensions.FormatUnitName(ci.InternalNumber, ci.DisplayName),
                    ci.Amount,
                    ci.Percentage,
                })
                .ToList()
            : [];

        var chargeItemsByCharge = chargeItems
            .GroupBy(ci => ci.ChargeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ── 4. Raggruppa per conto → spesa → allocazioni ─────────────────────
        var accountGroups = expenses
            .GroupBy(e => e.AccountId)
            .Select(ag =>
            {
                var first = ag.First();
                var expenseRows = ag
                    .Select(e =>
                    {
                        var allocs = allocationsByExpense.TryGetValue(e.ExpenseId, out var list)
                            ? list.Select(a => new ConsuntivoAllocationRowDto
                            {
                                UnitId          = a.UnitId,
                                UnitName        = a.UnitName,
                                Millesimal      = a.Millesimal,
                                AllocatedAmount = a.AllocatedAmount,
                            })
                            .OrderBy(a => a.UnitName)
                            .ToList()
                            : [];

                        return new ConsuntivoExpenseRowDto
                        {
                            ExpenseId    = e.ExpenseId,
                            Name         = e.ExpenseName,
                            GrossAmount  = e.GrossAmount,
                            SupplierName = e.SupplierName,
                            DocumentDate = e.DocumentDate ?? default,
                            Allocations  = allocs,
                        };
                    })
                    .OrderBy(e => e.DocumentDate)
                    .ToList();

                // Per i conti a consumo il totale è la somma delle quote consumi
                // approvate (fonte autoritativa), non la somma delle bollette.
                decimal totalAmount = consumptionAccountIds.Contains(ag.Key)
                    ? consumptionByAccountUnit.Where(kv => kv.Key.AccountId == ag.Key).Sum(kv => kv.Value)
                    : ag.Sum(e => e.GrossAmount);

                return new ConsuntivoAccountRowDto
                {
                    AccountId   = ag.Key,
                    AccountCode = first.AccountCode,
                    AccountName = first.AccountName,
                    Level       = first.AccountLevel,
                    ParentId    = first.ParentAccountId,
                    TotalAmount = totalAmount,
                    Expenses    = expenseRows,
                };
            })
            .OrderBy(a => a.AccountCode)
            .ToList();

        // ── 5. Aggiungi le ConsumptionCharge senza Expense come conto virtuale ─
        // Le raggruppiamo sotto un conto sintetico "Consumi (senza conto)"
        // oppure, se il tipo consumo ha un nome riconoscibile, lo usiamo come label.
        if (chargesWithoutExpense.Any())
        {
            // Raggruppa per tipo consumo (ognuno diventa una "voce spesa" virtuale)
            var virtualExpenseRows = chargesWithoutExpense
                .Select(c =>
                {
                    var allocs = chargeItemsByCharge.TryGetValue(c.ChargeId, out var items)
                        ? items.Select(ci => new ConsuntivoAllocationRowDto
                        {
                            UnitId          = ci.UnitId,
                            UnitName        = ci.UnitName,
                            Millesimal      = 0m,
                            AllocatedAmount = ci.Amount,
                        })
                        .OrderBy(a => a.UnitName)
                        .ToList()
                        : [];

                    return new ConsuntivoExpenseRowDto
                    {
                        ExpenseId    = -c.ChargeId,   // id negativo per distinguere da spese reali
                        Name         = c.ChargeName,
                        GrossAmount  = c.TotalAmount,
                        SupplierName = null,
                        DocumentDate = c.DocumentDate,
                        Allocations  = allocs,
                    };
                })
                .OrderBy(e => e.DocumentDate)
                .ToList();

            // Cerca se esiste già un conto "consumi" tra quelli presenti,
            // altrimenti aggiunge un gruppo virtuale con id=0
            accountGroups.Add(new ConsuntivoAccountRowDto
            {
                AccountId   = 0,
                AccountCode = "—",
                AccountName = "Consumi (senza conto)",
                Level       = 1,
                ParentId    = null,
                TotalAmount = chargesWithoutExpense.Sum(c => c.TotalAmount),
                Expenses    = virtualExpenseRows,
            });
        }

        // ── 5b. Proprietari per unità (per l'ordinamento alfabetico per nominativo) ─
        var ownersByUnit = (await session.Query<UnitOwner>()
                .Where(o => o.Unit.Condominium.Id == condominiumId && o.IsActive && !o.IsDeleted)
                .Select(o => new { UnitId = o.Unit.Id, o.LastName, o.FirstName })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false))
            .GroupBy(o => o.UnitId)
            .ToDictionary(g => g.Key, g => string.Join("-", g.Select(x => Surname(x.LastName, x.FirstName))));

        // ── 6. Billing groups del condominio ─────────────────────────────────────
        var billingGroups = await session.Query<BillingGroup>()
            .Where(bg => bg.Condominium.Id == condominiumId && !bg.IsDeleted)
            .FetchMany(bg => bg.Units)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var unitToGroup = billingGroups
            .SelectMany(bg => bg.Units.Select(u => (UnitId: u.Id, Group: bg)))
            .ToDictionary(x => x.UnitId, x => x.Group);

        // ── 7. Quote (rate) emesse per l'esercizio — aggregato per unità ────────
        var feesByUnit = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.FiscalYear.Id == fiscalYearId && !f.IsDeleted)
            .Select(f => new
            {
                UnitId     = f.Unit.Id,
                AmountDue  = f.AmountDue,
                AmountPaid = f.AmountPaid,
                Balance    = f.Balance,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var feesByUnitDict = feesByUnit
            .GroupBy(f => f.UnitId)
            .ToDictionary(
                g => g.Key,
                g => (Due: g.Sum(f => f.AmountDue), Paid: g.Sum(f => f.AmountPaid), Balance: g.Sum(f => f.Balance))
            );

        // ── 8. Vista per unità (pivot) ────────────────────────────────────────
        // Allocazioni da ExpenseAllocation per i conti NON a consumo. Per i conti a
        // consumo gli importi per unità arrivano invece dalle ConsumptionChargeItem
        // (fonte autoritativa) → riconciliazione al centesimo col Bilancio.
        var allAllocRows = allocations
            .Select(a => new { a.UnitId, a.UnitName, a.AllocatedAmount,
                AccountId = expenses.First(e => e.ExpenseId == a.ExpenseId).AccountId })
            .Where(a => !consumptionAccountIds.Contains(a.AccountId))
            .Concat(consumptionItemRows.Select(ci => new
            {
                ci.UnitId,
                ci.UnitName,
                AllocatedAmount = ci.Amount,
                AccountId       = ci.AccountId,
            }))
            .ToList();

        // Aggiungi allocazioni dalle charge senza expense (AccountId = 0)
        var chargeAllocRows = chargeItems
            .Select(ci => new { ci.UnitId, ci.UnitName, AllocatedAmount = ci.Amount, AccountId = 0 })
            .ToList();

        // Unisce tutte le unità: quelle con allocazioni + quelle con solo fee (senza spese ripartite)
        var allUnitIds = allAllocRows.Select(a => a.UnitId)
            .Concat(chargeAllocRows.Select(ci => ci.UnitId))
            .Concat(feesByUnitDict.Keys)
            .Distinct()
            .ToList();

        // Mappa unitId → nome (dalle allocazioni; per le unità senza allocazioni recupera dal dict fee)
        var unitNameMap = allAllocRows.Select(a => new { a.UnitId, a.UnitName })
            .Concat(chargeAllocRows.Select(ci => new { ci.UnitId, ci.UnitName }))
            .GroupBy(x => x.UnitId)
            .ToDictionary(g => g.Key, g => g.First().UnitName);

        ConsuntivoUnitRowDto BuildUnitRow(int unitId)
        {
            var allocsForUnit = allAllocRows.Where(a => a.UnitId == unitId)
                .Concat(chargeAllocRows.Where(ci => ci.UnitId == unitId))
                .ToList();

            var entries = allocsForUnit
                .GroupBy(x => x.AccountId)
                .Select(cg =>
                {
                    var acct = accountGroups.FirstOrDefault(a => a.AccountId == cg.Key);
                    return new ConsuntivoUnitAccountEntryDto
                    {
                        AccountId       = cg.Key,
                        AccountCode     = acct?.AccountCode,
                        AccountName     = acct?.AccountName,
                        AllocatedAmount = cg.Sum(x => x.AllocatedAmount),
                    };
                })
                .OrderBy(e => e.AccountCode)
                .ToList();

            feesByUnitDict.TryGetValue(unitId, out var fees);

            return new ConsuntivoUnitRowDto
            {
                UnitId     = unitId,
                UnitName   = unitNameMap.TryGetValue(unitId, out var n) ? n : $"Unità {unitId}",
                Total      = allocsForUnit.Sum(a => a.AllocatedAmount),
                AmountDue  = fees.Due,
                AmountPaid = fees.Paid,
                Balance    = fees.Balance,
                Entries    = entries,
            };
        }

        // Raggruppa per billing group dove applicabile
        var allUnitRows    = new List<ConsuntivoUnitRowDto>();
        var groupedUnits   = new Dictionary<int, List<ConsuntivoUnitRowDto>>(); // groupId → sub-rows

        foreach (var unitId in allUnitIds)
        {
            if (unitToGroup.TryGetValue(unitId, out var grp))
            {
                if (!groupedUnits.ContainsKey(grp.Id)) groupedUnits[grp.Id] = new List<ConsuntivoUnitRowDto>();
                groupedUnits[grp.Id].Add(BuildUnitRow(unitId));
            }
            else
            {
                allUnitRows.Add(BuildUnitRow(unitId));
            }
        }

        // Chiave di ordinamento alfabetico: nominativo proprietario (A→Z),
        // fallback al nome unità per le unità senza proprietario.
        string UnitSortKey(ConsuntivoUnitRowDto r) =>
            r.IsGroup
                ? (r.BillingGroupName ?? string.Empty)
                : (ownersByUnit.TryGetValue(r.UnitId, out var on) && !string.IsNullOrWhiteSpace(on)
                    ? on
                    : (r.UnitName ?? string.Empty));

        foreach (var (groupId, subRows) in groupedUnits)
        {
            var grp = billingGroups.First(bg => bg.Id == groupId);
            var orderedSubRows = subRows
                .OrderBy(UnitSortKey, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            allUnitRows.Add(new ConsuntivoUnitRowDto
            {
                UnitId           = 0,
                UnitName         = grp.Name,
                IsGroup          = true,
                BillingGroupId   = grp.Id,
                BillingGroupName = grp.Name,
                Total            = orderedSubRows.Sum(r => r.Total),
                AmountDue        = orderedSubRows.Sum(r => r.AmountDue),
                AmountPaid       = orderedSubRows.Sum(r => r.AmountPaid),
                Balance          = orderedSubRows.Sum(r => r.Balance),
                Entries          = orderedSubRows
                    .SelectMany(r => r.Entries)
                    .GroupBy(e => e.AccountId)
                    .Select(g =>
                    {
                        var first = g.First();
                        return new ConsuntivoUnitAccountEntryDto
                        {
                            AccountId       = g.Key,
                            AccountCode     = first.AccountCode,
                            AccountName     = first.AccountName,
                            AllocatedAmount = g.Sum(e => e.AllocatedAmount),
                        };
                    })
                    .OrderBy(e => e.AccountCode)
                    .ToList(),
                Units = orderedSubRows,
            });
        }

        allUnitRows = allUnitRows
            .OrderBy(UnitSortKey, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new ConsuntivoDetailDto
        {
            BudgetId       = command.BudgetId,
            FiscalYear     = budgetRow.FiscalYearCode,
            HasAllocations = allocations.Any() || chargeItems.Any(),
            Accounts       = accountGroups,
            Units          = allUnitRows,
        };
    }

    private static string Surname(string? lastName, string? firstName)
    {
        var ln = (lastName ?? "").Trim();
        if (ln.Length > 0) return ln;
        return (firstName ?? "").Trim();
    }
}
