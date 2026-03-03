using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BudgetItem;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateBudgetItemCommandConsumer
    : InMemoryConsumerBase<UpdateBudgetItemCommand, BudgetItemReadDto>
{
    private readonly IBudgetItemService      _budgetItemService;
    private readonly IBudgetService          _budgetService;
    private readonly IChartOfAccountsService _accountService;
    private readonly IUserService            _userService;

    public UpdateBudgetItemCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetItemService budgetItemService,
        IBudgetService budgetService,
        IChartOfAccountsService accountService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetItemService = budgetItemService;
        _budgetService     = budgetService;
        _accountService    = accountService;
        _userService       = userService;
    }

    protected override async Task<BudgetItemReadDto> Consume(
        UpdateBudgetItemCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var existing = await _budgetItemService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (existing == null) return null;

        var account = await _accountService
            .GetByIdAsync(command.Dto.AccountId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        existing.ApplyUpdate(command.Dto, account);

        var updated = await _budgetItemService
            .UpdateAsync(existing, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Ricalcola totale spese del budget
        var budget = existing.Budget;
        var allItems = await _budgetItemService
            .GetByBudgetIdAsync(budget.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        budget.TotalExpenses = allItems.Sum(i => i.Amount);
        await _budgetService.UpdateAsync(budget, currentUser, cancellationToken).ConfigureAwait(false);

        return updated.ToReadDto();
    }
}
