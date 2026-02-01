using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.Currency;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using MassTransit;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Currency;

public class UpdateCurrencyCommandConsumer : InMemoryConsumerBase<UpdateCurrencyCommand, CurrencyReadDto>
{
    private readonly ICurrencyService _currencyService;
    private readonly IUserService _userService;

    public UpdateCurrencyCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ICurrencyService currencyService, IUserService userService) : base(sessionFactoryProvider)
    {
        _currencyService = currencyService;
        _userService = userService;
    }

    protected override async Task<CurrencyReadDto> Consume(UpdateCurrencyCommand @event, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(@event.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);



        var x = await _currencyService.Update(@event.CurrencyId,
            @event.Item, currentUser, cancellationToken).ConfigureAwait(false);

        return x.ToDto();
    }
}