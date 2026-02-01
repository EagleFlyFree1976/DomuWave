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
    SetPaymentMethodDefaultForAccountTypeConsumer : InMemoryConsumerBase<SetPaymentMethodDefaultForAccountType, bool>
{
    private readonly IAccountTypeService _accountTypeService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;

    public SetPaymentMethodDefaultForAccountTypeConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountTypeService accountTypeService, IUserService userService, IMediator mediator, ICoreAuthorizationManager authorizationManager) : base(sessionFactoryProvider)
    {
        _accountTypeService = accountTypeService;
        _userService = userService;
        _mediator = mediator;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<bool> Consume(SetPaymentMethodDefaultForAccountType evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (!await _authorizationManager.CanCreate(currentUser, AuthorizationKeys.Admin, AuthorizationKeys.DomuWaveModule, cancellationToken).ConfigureAwait(false))
        {
            throw new NotAllowedOperationException("Non hai i permessi per poter eseguire l'operazione richiesta");
        }

        var allAssociationForAccountType = await session.Query<AccountTypePaymentMethod>()
            .Where(k => !k.IsDeleted && k.AccountType.Id == evt.AccountTypeId).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var newDefault = allAssociationForAccountType.FirstOrDefault(l => l.PaymentMethod.Id == evt.PaymentMethodId);
        if (newDefault == null)
        {
            throw new NotFoundException("Metodo di pagamento non definito sulla tipologia di account specificata");
        }


        var oldDefault = allAssociationForAccountType.FirstOrDefault(l => l.IsDefault);
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