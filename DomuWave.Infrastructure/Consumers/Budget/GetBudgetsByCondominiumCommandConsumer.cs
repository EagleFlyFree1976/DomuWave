using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetBudgetsByCondominiumCommandConsumer
    : InMemoryConsumerBase<GetBudgetsByCondominiumCommand, IList<BudgetReadDto>>
{
    private readonly IBudgetService _budgetService;
    private readonly IUserService   _userService;

    public GetBudgetsByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService budgetService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetService = budgetService;
        _userService   = userService;
    }

    protected override async Task<IList<BudgetReadDto>> Consume(
        GetBudgetsByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budgets = await _budgetService
            .GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return budgets.Select(b => b.ToReadDto()).ToList();
    }
}
