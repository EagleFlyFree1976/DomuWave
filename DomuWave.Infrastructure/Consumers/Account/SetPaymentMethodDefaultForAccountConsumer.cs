using DomuWave.Services.Command.Book;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.AccountType;

/// <summary>
///  Imposta il metodo di pagamento di default per l'account type specificato
/// </summary>
public class
    SetPaymentMethodDefaultForAccountConsumer : InMemoryConsumerBase<SetPaymentMethodDefaultForAccount, bool>
{
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;

    public SetPaymentMethodDefaultForAccountConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountService accountService, IUserService userService, IMediator mediator, ICoreAuthorizationManager authorizationManager) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService = userService;
        _mediator = mediator;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<bool> Consume(SetPaymentMethodDefaultForAccount evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        

        var associationForAccount = await session.Query<AccountPaymentMethod>()
            .Where(k => !k.IsDeleted && k.Account.Id == evt.AccountId).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var newDefault = associationForAccount.FirstOrDefault(l => l.PaymentMethod.Id == evt.PaymentMethodId);
        if (newDefault == null)
        {
            throw new NotFoundException("Metodo di pagamento non definito sull'account specificato");
        }


        var oldDefault = associationForAccount.FirstOrDefault(l => l.IsDefault);
        if (oldDefault != null)
        {
            oldDefault.IsDefault = false;
            await session.SaveOrUpdateAsync(oldDefault, cancellationToken).ConfigureAwait(false);
        }

        newDefault.IsDefault = true;
        newDefault.Trace(currentUser);
        await session.SaveOrUpdateAsync(newDefault, cancellationToken).ConfigureAwait(false);

        return true;
    }
}