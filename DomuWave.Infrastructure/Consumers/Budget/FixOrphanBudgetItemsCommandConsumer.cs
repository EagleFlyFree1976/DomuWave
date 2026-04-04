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

public class FixOrphanBudgetItemsCommandConsumer
    : InMemoryConsumerBase<FixOrphanBudgetItemsCommand, int>
{
    private readonly IBudgetService  _budgetService;
    private readonly IUserService    _userService;

    public FixOrphanBudgetItemsCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService          budgetService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _budgetService = budgetService;
        _userService   = userService;
    }

    protected override async Task<int> Consume(
        FixOrphanBudgetItemsCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await _budgetService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (budget == null)
            throw new NotFoundException("Budget non trovato.");

        // Carica tutti gli item attivi del budget (proiezione scalare)
        var itemProjections = await session.Query<BudgetItem>()
            .Where(x => x.Budget.Id == command.Id && !x.IsDeleted)
            .Select(x => new { x.Id, AccountId = x.Account.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var presentAccountIds = new HashSet<int>(itemProjections.Select(x => x.AccountId));

        // Carica tutti gli account del condominio con il loro parent (proiezione scalare)
        var accountProjections = await session.Query<ChartOfAccounts>()
            .Where(a => a.Condominium.Id == budget.Condominium.Id && !a.IsDeleted)
            .Select(a => new { a.Id, a.Code, a.Name, ParentId = (int?)a.ParentAccount.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var accountById = accountProjections.ToDictionary(a => a.Id);

        // Per ogni item presente, risale la catena dei parent e raccoglie gli antenati mancanti
        var toInsert = new Dictionary<int, (string Code, string Name)>();

        foreach (var item in itemProjections)
        {
            if (!accountById.TryGetValue(item.AccountId, out var acc)) continue;

            var parentId = acc.ParentId;
            while (parentId.HasValue && accountById.TryGetValue(parentId.Value, out var parent))
            {
                if (!presentAccountIds.Contains(parent.Id) && !toInsert.ContainsKey(parent.Id))
                    toInsert[parent.Id] = (parent.Code, parent.Name);

                parentId = parent.ParentId;
            }
        }

        if (toInsert.Count == 0)
            return 0;

        // Crea i BudgetItem mancanti per gli antenati
        int created = 0;
        foreach (var (accountId, (code, name)) in toInsert)
        {
            var ancestorAccount = await session.GetAsync<ChartOfAccounts>(accountId, cancellationToken)
                .ConfigureAwait(false);
            if (ancestorAccount == null) continue;

            var newItem = new BudgetItem
            {
                Budget      = budget,
                Tenant      = budget.Tenant,
                Account     = ancestorAccount,
                AccountCode = code,
                AccountName = name,
                Name        = string.Empty,
                Amount      = 0,
            };
            newItem.Trace(currentUser);
            await session.SaveAsync(newItem, cancellationToken).ConfigureAwait(false);
            created++;
        }

        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return created;
    }
}
