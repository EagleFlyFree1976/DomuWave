using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetExpensesByCondominiumCommandConsumer : InMemoryConsumerBase<GetExpensesByCondominiumCommand, IList<Expense>>
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService _userService;

    public GetExpensesByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExpenseService expenseService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _expenseService = expenseService;
        _userService = userService;
    }

    protected override async Task<IList<Expense>> Consume(
        GetExpensesByCondominiumCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _expenseService
            .GetByCondominiumIdAsync(command.CondominiumId, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
