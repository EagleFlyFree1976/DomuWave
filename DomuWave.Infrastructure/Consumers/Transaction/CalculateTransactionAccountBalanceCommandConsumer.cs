using DomuWave.Services.Command.Transaction;
using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Transaction;

public class CalculateTransactionAccountBalanceCommandConsumer : InMemoryConsumerBase<CalculateTransactionAccountBalanceCommand, decimal>
{
    private ITransactionService _transactionService;
    private IUserService _userService;
    public CalculateTransactionAccountBalanceCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ITransactionService transactionService, IUserService userService) : base(sessionFactoryProvider)
    {
        _transactionService = transactionService;
        _userService = userService;
    }
        
    protected override async Task<decimal> Consume(CalculateTransactionAccountBalanceCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);
        var balance = await _transactionService.CalculateAccountBalance(evt.TransactionId, evt.PrevAccountId, evt.ActualAccountId, evt.PrevTransactionDate, evt.CurrentTransactionDate, evt.BookId, currentUser, cancellationToken).ConfigureAwait(false);
        return balance;
    }
}