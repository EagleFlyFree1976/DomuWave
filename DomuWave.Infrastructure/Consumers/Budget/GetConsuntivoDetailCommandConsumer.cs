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

        // ── 1. Carica tutte le spese dell'esercizio ──────────────────────────
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

        // ── 2. Carica le allocazioni per unità (se esistono) ─────────────────
        var expenseIds = expenses.Select(e => e.ExpenseId).ToList();

        var allocations = expenseIds.Any()
            ? await session.Query<ExpenseAllocation>()
                .Where(a => expenseIds.Contains(a.Expense.Id) && !a.IsDeleted)
                .Select(a => new
                {
                    ExpenseId       = a.Expense.Id,
                    UnitId          = a.Unit.Id,
                    UnitName        = a.Unit.DisplayName ?? a.Unit.InternalNumber,
                    a.Millesimal,
                    a.AllocatedAmount,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false)
            : [];

        var allocationsByExpense = allocations
            .GroupBy(a => a.ExpenseId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ── 3. Raggruppa per conto → spesa → allocazioni ─────────────────────
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

        // ── 4. Vista per unità (solo se ci sono allocazioni) ─────────────────
        var unitGroups = allocations
            .GroupBy(a => a.UnitId)
            .Select(ug =>
            {
                var uf = ug.First();
                // Per ogni unità raggruppa per conto (via expense)
                var entries = ug
                    .Join(expenses, a => a.ExpenseId, e => e.ExpenseId, (a, e) => new { a, e })
                    .GroupBy(x => x.e.AccountId)
                    .Select(cg =>
                    {
                        var cf = cg.First();
                        return new ConsuntivoUnitAccountEntryDto
                        {
                            AccountId       = cg.Key,
                            AccountCode     = cf.e.AccountCode,
                            AccountName     = cf.e.AccountName,
                            AllocatedAmount = cg.Sum(x => x.a.AllocatedAmount),
                        };
                    })
                    .OrderBy(e => e.AccountCode)
                    .ToList();

                return new ConsuntivoUnitRowDto
                {
                    UnitId   = ug.Key,
                    UnitName = uf.UnitName,
                    Total    = ug.Sum(a => a.AllocatedAmount),
                    Entries  = entries,
                };
            })
            .OrderBy(u => u.UnitName)
            .ToList();

        return new ConsuntivoDetailDto
        {
            BudgetId      = command.BudgetId,
            FiscalYear    = budgetRow.FiscalYearCode,
            HasAllocations = allocations.Any(),
            Accounts      = accountGroups,
            Units         = unitGroups,
        };
    }
}
