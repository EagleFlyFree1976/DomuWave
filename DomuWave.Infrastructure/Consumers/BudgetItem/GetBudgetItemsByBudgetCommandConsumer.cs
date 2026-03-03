using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.BudgetItem;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetBudgetItemsByBudgetCommandConsumer
    : InMemoryConsumerBase<GetBudgetItemsByBudgetCommand, IList<BudgetItemReadDto>>
{
    private readonly IBudgetItemService _budgetItemService;
    private readonly IUserService       _userService;

    public GetBudgetItemsByBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetItemService budgetItemService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetItemService = budgetItemService;
        _userService       = userService;
    }

    protected override async Task<IList<BudgetItemReadDto>> Consume(
        GetBudgetItemsByBudgetCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var items = await _budgetItemService
            .GetByBudgetIdAsync(command.BudgetId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return items.Select(i => i.ToReadDto()).ToList();
    }
}
