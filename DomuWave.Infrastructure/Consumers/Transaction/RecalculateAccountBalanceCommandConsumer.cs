using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Transaction;

public class RecalculateAccountBalanceCommandConsumer : InMemoryConsumerBase<RecalculateAccountBalanceCommand>
{
    private ITransactionService _transactionService;
    private IUserService _userService;

    public RecalculateAccountBalanceCommandConsumer(ISessionFactoryProvider sessionFactoryProvider,
        ITransactionService transactionService, IUserService userService) : base(sessionFactoryProvider)
    {
        _transactionService = transactionService;
        _userService = userService;
    }

    protected override async Task Consume(RecalculateAccountBalanceCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {

        await _transactionService.RecalculateAccountBalance(evt.AccountId, cancellationToken).ConfigureAwait(false);

    }
}