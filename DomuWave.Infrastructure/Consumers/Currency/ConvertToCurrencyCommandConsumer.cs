using DomuWave.Services.Command.Book;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency;

public class ConvertToCurrencyCommandConsumer : InMemoryConsumerBase<ConvertToCurrencyCommand, ConvertResult>
{
    private ICurrencyService _currencyService;
    private IUserService _userService;
    public ConvertToCurrencyCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ICurrencyService currencyService, IUserService userService) : base(sessionFactoryProvider)
    {
        _currencyService = currencyService;
        _userService = userService;
    }

    protected override async Task<ConvertResult> Consume(ConvertToCurrencyCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        return await _currencyService
            .ConvertTo(evt.Amount, evt.SourceCurrency, evt.DestinationCurrency, evt.TargetDate,currentUser, cancellationToken)
            .ConfigureAwait(false);
    }
}