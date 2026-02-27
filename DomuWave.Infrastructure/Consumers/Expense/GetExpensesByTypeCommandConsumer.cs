using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetExpensesByTypeCommandConsumer : InMemoryConsumerBase<GetExpensesByTypeCommand, IList<Expense>>
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService _userService;

    public GetExpensesByTypeCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExpenseService expenseService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _expenseService = expenseService;
        _userService = userService;
    }

    protected override async Task<IList<Expense>> Consume(
        GetExpensesByTypeCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return await _expenseService
            .GetByTypeAsync(command.CondominiumId, command.ExpenseType, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
