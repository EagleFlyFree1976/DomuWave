using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BudgetItem;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CreateBudgetItemCommandConsumer
    : InMemoryConsumerBase<CreateBudgetItemCommand, BudgetItemReadDto>
{
    private readonly IBudgetItemService        _budgetItemService;
    private readonly IBudgetService            _budgetService;
    private readonly IChartOfAccountsService   _accountService;
    private readonly IUserService              _userService;

    public CreateBudgetItemCommandConsumer(
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
        CreateBudgetItemCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await _budgetService
            .GetByIdAsync(command.Dto.BudgetId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var account = await _accountService
            .GetByIdAsync(command.Dto.AccountId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        var item = new BudgetItem
        {
            Budget = budget,
            Tenant = budget.Tenant,
            Account = account,
            Name   = command.Dto.Description ?? string.Empty,
            Amount = command.Dto.Amount,
            Notes  = command.Dto.Notes,
        };

        var created = await _budgetItemService
            .CreateAsync(item, currentUser, cancellationToken)
            .ConfigureAwait(false);

        // Ricalcola totale spese del budget
        var allItems = await _budgetItemService
            .GetByBudgetIdAsync(budget.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);
        budget.TotalExpenses = allItems.Sum(i => i.Amount);
        await _budgetService.UpdateAsync(budget, currentUser, cancellationToken).ConfigureAwait(false);

        return created.ToReadDto();
    }
}
