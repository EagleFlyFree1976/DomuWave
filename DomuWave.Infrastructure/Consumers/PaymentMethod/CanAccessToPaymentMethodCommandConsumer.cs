using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using SimpleMediator.Core;

namespace DomuWave.Services.Command.PaymentMethod;

/// <summary>
///  Ritorna il dettaglio dell'account selezionato
/// </summary>

public class CanAccessToPaymentMethodCommandConsumer : InMemoryConsumerBase<CanAccessToPaymentMethodCommand, bool>
{
    private ICoreAuthorizationManager _authorizationManager;
    private IMediator _mediator;
    private IBookService _bookService;
    private IPaymentMethodService _paymentMethodService;
    private IUserService _userService;

    public CanAccessToPaymentMethodCommandConsumer(ISessionFactoryProvider sessionFactoryProvider, ICoreAuthorizationManager authorizationManager, IMediator mediator, IBookService bookService, IUserService userService, IPaymentMethodService paymentMethodService) : base(sessionFactoryProvider)
    {
        _authorizationManager = authorizationManager;
        _mediator = mediator;
        _bookService = bookService;
        _userService = userService;
        _paymentMethodService = paymentMethodService;
    }

    protected override async Task<bool> Consume(CanAccessToPaymentMethodCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);
        var book = await _bookService.GetById(evt.BookId, cancellationToken).ConfigureAwait(false);

        if (book == null)
            return false;
        if (book.OwnerId == currentUser.Id)
        {
            if (evt.PaymentMethodId.HasValue)
            {
                var paymentMethod = await _paymentMethodService
                    .GetById(evt.PaymentMethodId.Value, currentUser, cancellationToken).ConfigureAwait(false);

                if (paymentMethod == null)
                    return false;
                book = paymentMethod.Book;
                if (book.OwnerId == currentUser.Id)
                    return true;
                else if (book.IsSystem)
                {
                    return await _authorizationManager
                        .CanAction(currentUser, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            return true;
        }
        if (book.IsSystem)
        {
            return await _authorizationManager
                .CanAction(currentUser, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        return false;
    }
}