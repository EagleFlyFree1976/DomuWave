using DomuWave.Services.Command.Currency.ExchangeRate;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency.Exchange;

public class UpdateExchangeRateCommandConsumer : InMemoryConsumerBase<UpdateExchangeRateCommand, ExchangeRateHistoryReadDto>
{
    private IExchangeRateHistoryService _exchangeRateHistoryService;
    private IUserService _userService;
    public UpdateExchangeRateCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IExchangeRateHistoryService exchangeRateHistoryService, IUserService userService) : base(sessionFactoryProvider)
    {
        _exchangeRateHistoryService = exchangeRateHistoryService;
        _userService = userService;
    }

    protected override async Task<ExchangeRateHistoryReadDto> Consume(UpdateExchangeRateCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var result = await _exchangeRateHistoryService.Update(evt.ExchangeRateHistoryId,evt.Item, currentUser, cancellationToken)
            .ConfigureAwait(false);

        return result.ToDto();
        ;
    }
}