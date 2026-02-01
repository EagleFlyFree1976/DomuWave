using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

 
public class DeletePaymentMethodByIdCommandConsumer : InMemoryConsumerBase<DeletePaymentMethodByIdCommand, bool>
{
    private IUserService _userService;
    private IPaymentMethodService _paymentMethodService;

    public DeletePaymentMethodByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IPaymentMethodService paymentMethodService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _paymentMethodService = paymentMethodService;
    }

    protected override async Task<bool> Consume(DeletePaymentMethodByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        await _paymentMethodService.Delete(evt.PaymentMethodId, evt.BookId,currentUser, cancellationToken).ConfigureAwait(false);
        return true;

    }
}