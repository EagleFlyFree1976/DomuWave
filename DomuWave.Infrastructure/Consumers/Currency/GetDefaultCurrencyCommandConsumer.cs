using DomuWave.Services.Command.Book;
using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency;

public class GetDefaultCurrencyCommandConsumer : InMemoryConsumerBase<GetDefaultCurrencyCommand, Models.Currency>
{
    private readonly ICurrencyService _currencyService;
    private readonly IUserService _userService;

    public GetDefaultCurrencyCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ICurrencyService currencyService, IUserService userService) : base(sessionFactoryProvider)
    {
        _currencyService = currencyService;
        _userService = userService;
    }

    protected override async Task<Models.Currency> Consume(GetDefaultCurrencyCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        return (await _currencyService.GetDefault(currentUser, cancellationToken).ConfigureAwait(false));
    }
}