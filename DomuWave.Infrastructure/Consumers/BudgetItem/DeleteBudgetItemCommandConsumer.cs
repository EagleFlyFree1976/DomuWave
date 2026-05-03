using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BudgetItem;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class DeleteBudgetItemCommandConsumer
    : InMemoryConsumerBase<DeleteBudgetItemCommand, bool>
{
    private readonly IBudgetItemService _budgetItemService;
    private readonly IBudgetService     _budgetService;
    private readonly IUserService       _userService;

    public DeleteBudgetItemCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetItemService budgetItemService,
        IBudgetService budgetService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetItemService = budgetItemService;
        _budgetService     = budgetService;
        _userService       = userService;
    }

    protected override async Task<bool> Consume(
        DeleteBudgetItemCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var item = await _budgetItemService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (item == null) return false;

        var budget = item.Budget;
        var result = await _budgetItemService
            .DeleteAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Ricalcola totali del budget in base al tipo conto
        var allItems = await _budgetItemService
            .GetByBudgetIdAsync(budget.Id, budget.Tenant.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        var parentAccountIds = allItems.Where(i => i.Account?.ParentAccount != null)
            .Select(i => i.Account.ParentAccount.Id).ToHashSet();
        var leafItems = allItems.Where(i => i.Account != null && !parentAccountIds.Contains(i.Account.Id)).ToList();
        budget.TotalExpenses = leafItems.Where(i => i.Account.Type == ChartOfAccountsType.Uscita).Sum(i => i.Amount);
        budget.TotalIncome   = leafItems.Where(i => i.Account.Type != ChartOfAccountsType.Uscita).Sum(i => i.Amount);
        await _budgetService.UpdateAsync(budget, currentUser, cancellationToken).ConfigureAwait(false);

        return result;
    }
}
