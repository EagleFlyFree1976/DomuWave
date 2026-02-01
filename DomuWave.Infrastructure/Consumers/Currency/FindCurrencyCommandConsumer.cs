using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency;

public class FindCurrencyCommandConsumer : InMemoryConsumerBase<FindCurrencyCommand, IList<CurrencyReadDto>>
{
    private readonly ICurrencyService _currencyService;
    private readonly IUserService _userService;

    public FindCurrencyCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ICurrencyService currencyService, IUserService userService) : base(sessionFactoryProvider)
    {
        _currencyService = currencyService;
        _userService = userService;
    }

    protected override async Task<IList<CurrencyReadDto>> Consume(FindCurrencyCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        IList<Models.Currency> currencies = await _currencyService.Find(@event.Q,currentUser, cancellationToken).ConfigureAwait(false);

        return currencies.Select(j=>j.ToDto()).ToList();
    }
}