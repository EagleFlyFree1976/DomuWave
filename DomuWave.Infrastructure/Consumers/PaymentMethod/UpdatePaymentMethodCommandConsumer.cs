using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;
 

/// <summary>
/// Crea un nuovo metodo di pagamento con im parametri impostati
/// </summary>
public class UpdatePaymentMethodCommandConsumer : InMemoryConsumerBase<UpdatePaymentMethodCommand, PaymentMethodReadDto>
{
    private IUserService _userService;
    private IPaymentMethodService _paymentMethodService;

    public UpdatePaymentMethodCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IPaymentMethodService paymentMethodService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _paymentMethodService = paymentMethodService;
    }

    protected override async Task<PaymentMethodReadDto> Consume(UpdatePaymentMethodCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        Models.PaymentMethod x = await _paymentMethodService.Update(evt.PaymentMethodId,evt.UpdateDto, currentUser, cancellationToken).ConfigureAwait(false);
        return x.ToDto();

    }
}
