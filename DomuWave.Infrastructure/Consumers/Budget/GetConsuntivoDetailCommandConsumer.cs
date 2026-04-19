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
                GrossAmount     = e.GrossAmount,
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

        // ── 3. ConsumptionCharge approvate SENZA Expense collegata ───────────
        // Ripartizioni create prima dell'introduzione del campo Account su ConsumptionType:
        // non hanno una Expense e non possono averne una (Account mancante).
        // Le includiamo direttamente come voci sintetiche nel dettaglio.
        var chargesWithoutExpense = await session.Query<ConsumptionCharge>()
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
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

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
                            DocumentDate = e.DocumentDate,
                            Allocations  = allocs,
                        };
                    })
                    .OrderBy(e => e.DocumentDate)
                    .ToList();

                return new ConsuntivoAccountRowDto
                {
                    AccountId   = ag.Key,
                    AccountCode = first.AccountCode,
                    AccountName = first.AccountName,
                    Level       = first.AccountLevel,
                    ParentId    = first.ParentAccountId,
                    TotalAmount = ag.Sum(e => e.GrossAmount),
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
        // Combina allocazioni da ExpenseAllocation e da ConsumptionChargeItem
        var allAllocRows = allocations
            .Select(a => new { a.UnitId, a.UnitName, a.AllocatedAmount,
                AccountId = expenses.First(e => e.ExpenseId == a.ExpenseId).AccountId })
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

        foreach (var (groupId, subRows) in groupedUnits)
        {
            var grp = billingGroups.First(bg => bg.Id == groupId);
            allUnitRows.Add(new ConsuntivoUnitRowDto
            {
                UnitId           = 0,
                UnitName         = grp.Name,
                IsGroup          = true,
                BillingGroupId   = grp.Id,
                BillingGroupName = grp.Name,
                Total            = subRows.Sum(r => r.Total),
                AmountDue        = subRows.Sum(r => r.AmountDue),
                AmountPaid       = subRows.Sum(r => r.AmountPaid),
                Balance          = subRows.Sum(r => r.Balance),
                Entries          = subRows
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
                Units = subRows,
            });
        }

        allUnitRows = allUnitRows.OrderBy(u => u.IsGroup ? u.BillingGroupName : u.UnitName).ToList();

        return new ConsuntivoDetailDto
        {
            BudgetId       = command.BudgetId,
            FiscalYear     = budgetRow.FiscalYearCode,
            HasAllocations = allocations.Any() || chargeItems.Any(),
            Accounts       = accountGroups,
            Units          = allUnitRows,
        };
    }
}
