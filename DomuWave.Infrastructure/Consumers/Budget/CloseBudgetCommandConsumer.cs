using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class CloseBudgetCommandConsumer
    : InMemoryConsumerBase<CloseBudgetCommand, bool>
{
    private readonly IBudgetService _budgetService;
    private readonly IUserService   _userService;

    public CloseBudgetCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService budgetService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetService = budgetService;
        _userService   = userService;
    }

    protected override async Task<bool> Consume(
        CloseBudgetCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _budgetService
            .CloseBudgetAsync(command.Id, command.CurrentUserId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
