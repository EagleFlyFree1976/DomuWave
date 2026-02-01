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
/// ritorna l'elenco di tutti i metodi di pagamento fanno riferimento ad un dato book oppure tutti quelli di default
/// </summary>

public class FindPaymentMethodCommandConsumer : InMemoryConsumerBase<FindPaymentMethodCommand, IList<PaymentMethodReadDto>>
{
    private IUserService _userService;
    private IPaymentMethodService _paymentMethodService;

    public FindPaymentMethodCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IPaymentMethodService paymentMethodService) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _paymentMethodService = paymentMethodService;
    }

    protected override async Task<IList<PaymentMethodReadDto>> Consume(FindPaymentMethodCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        IList<Models.PaymentMethod> all = await _paymentMethodService.Find(evt.BookId, true, currentUser, cancellationToken).ConfigureAwait(false);

        return all.Select(j=>j.ToDto()).ToList();

    }
}