using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetBudgetByIdCommandConsumer
    : InMemoryConsumerBase<GetBudgetByIdCommand, BudgetReadDto>
{
    private readonly IBudgetService _budgetService;
    private readonly IUserService   _userService;

    public GetBudgetByIdCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IBudgetService budgetService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _budgetService = budgetService;
        _userService   = userService;
    }

    protected override async Task<BudgetReadDto> Consume(
        GetBudgetByIdCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var budget = await _budgetService
            .GetByIdAsync(command.Id, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return budget?.ToReadDto();
    }
}
