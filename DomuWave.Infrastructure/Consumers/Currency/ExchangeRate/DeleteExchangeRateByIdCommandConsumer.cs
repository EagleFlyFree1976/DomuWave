using DomuWave.Services.Command.Currency.ExchangeRate;
using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency.Exchange;

public class DeleteExchangeRateByIdCommandConsumer : InMemoryConsumerBase<DeleteExchangeRateByIdCommand, bool>
{
    private IExchangeRateHistoryService _exchangeRateHistoryService;
    private IUserService _userService;
    public DeleteExchangeRateByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IExchangeRateHistoryService exchangeRateHistoryService, IUserService userService) : base(sessionFactoryProvider)
    {
        _exchangeRateHistoryService = exchangeRateHistoryService;
        _userService = userService;
    }
    protected override async Task<bool> Consume(DeleteExchangeRateByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        await _exchangeRateHistoryService.Delete(evt.ExchangeRateId, currentUser, cancellationToken).ConfigureAwait(false);
        return true;
    }
}