using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models.Dto;
using DomuWave.Services.Models.Dto.PaymentMethod;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.PaymentMethod;

public class GetPaymentMethodByIdCommandConsumer : InMemoryConsumerBase<GetPaymentMethodByIdCommand, PaymentMethodReadDto>
{
    private IUserService _userService;
    private IPaymentMethodService _paymentMethodService;
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;
    public GetPaymentMethodByIdCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, IUserService userService, IPaymentMethodService paymentMethodService, ICoreAuthorizationManager authorizationManager, IMediator mediator) : base(sessionFactoryProvider)
    {
        _userService = userService;
        _paymentMethodService = paymentMethodService;
        _authorizationManager = authorizationManager;
        _mediator = mediator;
    }

    protected override async Task<PaymentMethodReadDto> Consume(GetPaymentMethodByIdCommand evt, IMediationContext mediationContext, CancellationToken cancellationToken)
    {
        var currentUser =
            await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        // posso accedere al bookid specificato?
        CanAccessToPaymentMethodCommand canAccessToPaymentMethodCommand =
            new CanAccessToPaymentMethodCommand(evt.PaymentMethodId,evt.CurrentUserId, evt.BookId);
        bool canAccess = await _mediator.GetResponse(canAccessToPaymentMethodCommand).ConfigureAwait(false);
        if (!canAccess)
        {
            throw new UserNotAuthorizedException("Non hai accesso alla risorsa richiesta");
        }
        

        Models.PaymentMethod x = await _paymentMethodService.GetById(evt.PaymentMethodId, currentUser, cancellationToken).ConfigureAwait(false);
        if (x == null)
            throw new NotFoundException("Elemento non trovato");

        


        return x.ToDto();

    }
}

