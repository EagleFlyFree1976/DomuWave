using DomuWave.Services.Command.Book;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Exceptions;
using CPQ.Core.Extensions;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Criterion;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.AccountType;

public class AssociatePaymentMethodToAccountTypeConsumer : InMemoryConsumerBase<AssociatePaymentMethodToAccountType, int>
{
    private readonly IAccountTypeService _accountTypeService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;

    public AssociatePaymentMethodToAccountTypeConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountTypeService accountTypeService, IUserService userService, IMediator mediator, ICoreAuthorizationManager authorizationManager) : base(sessionFactoryProvider)
    {
        _accountTypeService = accountTypeService;
        _userService = userService;
        _mediator = mediator;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<int> Consume(AssociatePaymentMethodToAccountType evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken).ConfigureAwait(false);

        if (!await _authorizationManager.CanCreate(currentUser, AuthorizationKeys.Admin,AuthorizationKeys.DomuWaveModule, cancellationToken).ConfigureAwait(false))
        {
            throw new NotAllowedOperationException("Non hai i permessi per poter eseguire l'operazione richiesta");
        }

        var exixts = await session.Query<AccountTypePaymentMethod>()
            .Where(k => k.AccountType.Id == evt.AccountTypeId && k.PaymentMethod.Id == evt.PaymentMethodId).AnyAsync(cancellationToken).ConfigureAwait(false);

        if (exixts)
        {
            throw new OxInvalidOperationException("Associazione già presente");
        }

        Models.AccountType accountType = await _accountTypeService
            .GetById(evt.AccountTypeId, currentUser, cancellationToken).ConfigureAwait(false);

        if (accountType == null)
        {
            throw new NotFoundException("Entità non trovata");
        }

        PaymentMethod paymentMethod = await session.GetAsync<PaymentMethod>(evt.PaymentMethodId, cancellationToken)
            .ConfigureAwait(false);

        if (paymentMethod == null)
        {
            throw new NotFoundException("Entità non trovata");
        }
        AccountTypePaymentMethod newAss = new AccountTypePaymentMethod();
        newAss.AccountType = accountType;
        newAss.PaymentMethod = paymentMethod;
        newAss.IsDefault = evt.IsDefault;
        newAss.Trace(currentUser);

         await session.SaveOrUpdateAsync(newAss, cancellationToken).ConfigureAwait(false);

         if (evt.IsDefault)
         {
             SetPaymentMethodDefaultForAccountType setPaymentMethodDefault =
                 new SetPaymentMethodDefaultForAccountType(evt.AccountTypeId, evt.PaymentMethodId, evt.CurrentUserId);
             await _mediator.GetResponse(setPaymentMethodDefault).ConfigureAwait(false);
         }

         return newAss.Id;
    }
}