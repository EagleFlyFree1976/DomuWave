using DomuWave.Services.Interfaces;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.PaymentMethod;

public class EnablePaymentMethodByIdCommandConsumer : InMemoryConsumerBase<EnablePaymentMethodByIdCommand, bool>
{
    private IUserService _userService;
    private IPaymentMethodService _paymentMethodService;

    public EnablePaymentMethodByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IPaymentMethodService paymentMethodService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _paymentMethodService = paymentMethodService;
    }

    protected override async Task<bool> Consume(EnablePaymentMethodByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        await _paymentMethodService.Enable(evt.PaymentMethodId, currentUser, cancellationToken).ConfigureAwait(false);
        return true;

    }
}