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

public class RecalculateBudgetItemsCommandConsumer
    : InMemoryConsumerBase<RecalculateBudgetItemsCommand, bool>
{
    private readonly IBudgetService     _budgetService;
    private readonly IBudgetItemService _budgetItemService;
    private readonly IUserService       _userService;

    public RecalculateBudgetItemsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService          budgetService,
        IBudgetItemService      budgetItemService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _budgetService     = budgetService;
        _budgetItemService = budgetItemService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        RecalculateBudgetItemsCommand command,
        IMediationContext              mediationContext,
        CancellationToken             cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budgetRow = await session.Query<Budget>()
            .Where(x => x.Id == command.Id && !x.IsDeleted)
            .Select(x => new { x.Id, x.Type, StatusId = x.Status.Id, CondominiumId = x.Condominium.Id, FiscalYearId = x.FiscalYear.Id, TenantId = x.Tenant.Id })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (budgetRow == null)
            throw new NotFoundException("Budget non trovato.");

        if (budgetRow.Type != BudgetType.Consuntivo)
            throw new ValidatorException("Il ricalcolo automatico è disponibile solo per i budget consuntivi.");

        if (budgetRow.StatusId == BudgetStatus.Closed)
            throw new ValidatorException("Non è possibile ricalcolare le voci di un budget chiuso.");

        // Carica l'entità completa per SaveOrUpdate
        var budget = await session.GetAsync<Budget>(command.Id, cancellationToken).ConfigureAwait(false);
        var condominiumId = budgetRow.CondominiumId;
        var fiscalYearId  = budgetRow.FiscalYearId;

        // ── 1. Soft-delete voci esistenti ────────────────────────────────────
        var existingItems = await session.Query<BudgetItem>()
            .Where(x => x.Budget.Id == budget.Id && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var item in existingItems)
        {
            item.IsDeleted = true;
            await session.SaveOrUpdateAsync(item, cancellationToken).ConfigureAwait(false);
        }
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // ── 2. Uscite: raggruppa Expense PAGATE per Account aggregando in SQL ──
        var expenseGroups = await session.Query<Expense>()
            .Where(x => x.Condominium.Id        == condominiumId
                     && x.FiscalYear.Id          == fiscalYearId
                     && x.PaymentStatus.Id       == ExpensePaymentStatus.Pagata
                     && !x.IsDeleted)
            .GroupBy(x => x.Account.Id)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(x => x.GrossAmount), Count = g.Count() })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Carica tutti i conti del condominio come proiezione scalare (evita lazy proxy)
        var allAccountRows = await session.Query<ChartOfAccounts>()
            .Where(a => a.Condominium.Id == condominiumId && !a.IsDeleted)
            .Select(a => new { a.Id, ParentAccountId = a.ParentAccount != null ? (int?)a.ParentAccount.Id : null })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var parentMap = allAccountRows.ToDictionary(a => a.Id, a => a.ParentAccountId); // id → parentId?

        // Accumula importi per account foglia + tutti gli antenati
        var amountByAccountId = new Dictionary<int, (decimal Total, int Count)>();
        foreach (var group in expenseGroups)
        {
            if (!parentMap.ContainsKey(group.AccountId)) continue;

            // Percorri la catena: foglia → padre → nonno → …
            int? currentId = group.AccountId;
            while (currentId.HasValue && parentMap.ContainsKey(currentId.Value))
            {
                if (amountByAccountId.TryGetValue(currentId.Value, out var existing))
                    amountByAccountId[currentId.Value] = (existing.Total + group.Total, existing.Count + group.Count);
                else
                    amountByAccountId[currentId.Value] = (group.Total, group.Count);

                currentId = parentMap[currentId.Value]; // sale al padre (null se radice)
            }
        }

        foreach (var (accountId, (total, count)) in amountByAccountId)
        {
            var account = await session.GetAsync<ChartOfAccounts>(accountId, cancellationToken)
                .ConfigureAwait(false);
            if (account == null) continue;

            var item = new BudgetItem
            {
                Budget      = budget,
                Tenant      = budget.Tenant,
                Account     = account,
                AccountCode = account.Code,
                AccountName = account.Name,
                Name        = account.Name ?? string.Empty,
                Amount      = total,
                Notes       = $"Ricalcolato automaticamente da {count} spese pagate",
            };
            item.Trace(currentUser);
            await session.SaveOrUpdateAsync(item, cancellationToken).ConfigureAwait(false);
        }

        // ── 3. Entrate: quote incassate (CondominiumFee.AmountPaid) per esercizio ─
        // Naviga via Installment (che ha FiscalYear e Condominium diretti) per evitare
        // navigazioni lazy profonde su Budget.FiscalYear che NHibernate non traduce.
        var totalPaid = await session.Query<CondominiumFee>()
            .Where(f => f.Installment.Condominium.Id == condominiumId
                     && f.Installment.FiscalYear.Id  == fiscalYearId
                     && !f.IsDeleted)
            .SumAsync(f => (decimal?)f.AmountPaid, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        if (totalPaid > 0)
        {
            // Cerca il conto Entrata dal preventivo approvato/chiuso per lo stesso esercizio
            var preventivoEntrataGroups = await session.Query<BudgetItem>()
                .Where(x => x.Budget.Condominium.Id == condominiumId
                         && x.Budget.FiscalYear.Id  == fiscalYearId
                         && x.Budget.Type           == BudgetType.Preventivo
                         && (x.Budget.Status.Id == BudgetStatus.Approved || x.Budget.Status.Id == BudgetStatus.Closed)
                         && x.Account.Type          == ChartOfAccountsType.Entrata
                         && !x.IsDeleted)
                .GroupBy(x => x.Account.Id)
                .Select(g => new { AccountId = g.Key, Total = g.Sum(x => x.Amount) })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Fallback: conti Entrata del piano dei conti del condominio (peso uniforme)
            if (!preventivoEntrataGroups.Any())
            {
                var fallbackIds = await session.Query<ChartOfAccounts>()
                    .Where(a => a.Condominium.Id == condominiumId
                             && a.Type           == ChartOfAccountsType.Entrata
                             && a.IsActive       && !a.IsDeleted)
                    .Select(a => a.Id)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                preventivoEntrataGroups = fallbackIds
                    .Select(id => new { AccountId = id, Total = 1m })
                    .ToList();
            }

            var totalWeight = preventivoEntrataGroups.Sum(x => x.Total);
            var allocated   = 0m;

            for (int i = 0; i < preventivoEntrataGroups.Count; i++)
            {
                var grp      = preventivoEntrataGroups[i];
                var srcAcct  = await session.GetAsync<ChartOfAccounts>(grp.AccountId, cancellationToken)
                    .ConfigureAwait(false);
                if (srcAcct == null) continue;

                decimal share;
                if (i == preventivoEntrataGroups.Count - 1)
                    share = totalPaid - allocated;
                else
                {
                    share      = totalWeight > 0 ? Math.Round(totalPaid * grp.Total / totalWeight, 2) : 0m;
                    allocated += share;
                }

                var item = new BudgetItem
                {
                    Budget      = budget,
                    Tenant      = budget.Tenant,
                    Account     = srcAcct,
                    AccountCode = srcAcct.Code,
                    AccountName = srcAcct.Name,
                    Name        = srcAcct.Name ?? string.Empty,
                    Amount      = share,
                    Notes       = "Entrate incassate (ricalcolo automatico)",
                };
                item.Trace(currentUser);
                await session.SaveOrUpdateAsync(item, cancellationToken).ConfigureAwait(false);
            }
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        // ── 4. Aggiorna i totali del budget (solo foglie) ────────────────────────
        // Un account è foglia se nessun altro account del condominio lo ha come ParentAccount.
        var allAccountsForTotals = await session.Query<ChartOfAccounts>()
            .Where(a => a.Condominium.Id == condominiumId && !a.IsDeleted)
            .Select(a => new { a.Id, TypeId = (int)a.Type, ParentId = a.ParentAccount != null ? (int?)a.ParentAccount.Id : null })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var parentIds    = allAccountsForTotals.Where(a => a.ParentId.HasValue).Select(a => a.ParentId!.Value).ToHashSet();
        var leafIds      = allAccountsForTotals.Where(a => !parentIds.Contains(a.Id)).Select(a => a.Id).ToHashSet();
        var uscitaLeafIds = allAccountsForTotals
            .Where(a => leafIds.Contains(a.Id) && a.TypeId == (int)ChartOfAccountsType.Uscita)
            .Select(a => a.Id).ToList();
        var nonUscitaLeafIds = allAccountsForTotals
            .Where(a => leafIds.Contains(a.Id) && a.TypeId != (int)ChartOfAccountsType.Uscita)
            .Select(a => a.Id).ToList();

        budget.TotalExpenses = await session.Query<BudgetItem>()
            .Where(x => x.Budget.Id == budget.Id && !x.IsDeleted && uscitaLeafIds.Contains(x.Account.Id))
            .SumAsync(x => (decimal?)x.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        budget.TotalIncome = await session.Query<BudgetItem>()
            .Where(x => x.Budget.Id == budget.Id && !x.IsDeleted && nonUscitaLeafIds.Contains(x.Account.Id))
            .SumAsync(x => (decimal?)x.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;
        budget.Trace(currentUser);
        await session.SaveOrUpdateAsync(budget, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
