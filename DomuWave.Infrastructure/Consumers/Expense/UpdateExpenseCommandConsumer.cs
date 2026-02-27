using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Interfaces;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class UpdateExpenseCommandConsumer : InMemoryConsumerBase<UpdateExpenseCommand, Expense>
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService _userService;

    public UpdateExpenseCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExpenseService expenseService,
        IUserService userService) : base(sessionFactoryProvider)
    {
        _expenseService = expenseService;
        _userService = userService;
    }

    protected override async Task<Expense> Consume(
        UpdateExpenseCommand command,
        IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var exists = await _expenseService
            .ExistsAsync(command.ExpenseId, currentUser, cancellationToken)
            .ConfigureAwait(false);
        if (!exists) return null;
        command.Entity.Id = command.ExpenseId;
        return await _expenseService
            .UpdateAsync(command.Entity, currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}
