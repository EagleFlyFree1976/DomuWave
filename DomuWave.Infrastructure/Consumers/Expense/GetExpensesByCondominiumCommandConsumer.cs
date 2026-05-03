using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Dto.Expense;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers;

public class GetExpensesByCondominiumCommandConsumer : InMemoryConsumerBase<GetExpensesByCondominiumCommand, IList<ExpenseReadDto>>
{
    private readonly IExpenseService _expenseService;
    private readonly IUserService    _userService;

    public GetExpensesByCondominiumCommandConsumer(
        ISessionFactoryProvider sessionFactoryProvider,
        IExpenseService         expenseService,
        IUserService            userService) : base(sessionFactoryProvider)
    {
        _expenseService = expenseService;
        _userService    = userService;
    }

    protected override async Task<IList<ExpenseReadDto>> Consume(
        GetExpensesByCondominiumCommand command,
        IMediationContext               mediationContext,
        CancellationToken               cancellationToken)
    {
        var currentUser = await _userService
            .GetByIdAsync(command.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var entities = await _expenseService
            .GetByCondominiumIdAsync(command.CondominiumId, command.TenantId, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return entities.Select(e => e.ToReadDto()).ToList();
    }
}
