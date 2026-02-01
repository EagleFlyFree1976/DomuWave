using DomuWave.Services.Command.Currency.ExchangeRate;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency.Exchange;

public class GetExchangeRateCommandConsumer : InMemoryConsumerBase<GetExchangeRateCommand, ExchangeRateHistoryReadDto>
{
    private IExchangeRateHistoryService _exchangeRateHistoryService;
    private IUserService _userService;
    public GetExchangeRateCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IExchangeRateHistoryService exchangeRateHistoryService, IUserService userService) : base(sessionFactoryProvider)
    {
        _exchangeRateHistoryService = exchangeRateHistoryService;
        _userService = userService;
    }
    
    protected override async Task<ExchangeRateHistoryReadDto> Consume(GetExchangeRateCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var result = await _exchangeRateHistoryService.GetExchangeRateAsync(evt.TargetDate.Date, evt.FromCurrencyId, evt.ToCurrencyId, evt.ExactlyMode, currentUser, cancellationToken).ConfigureAwait(false);
        return result.ToDto();
    }
}