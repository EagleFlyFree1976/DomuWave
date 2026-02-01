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

namespace DomuWave.Services.Consumers.Book;


public class AssociateAccountToPaymentMethodConsumer : InMemoryConsumerBase<AssociateAccountToPaymentMethod, long>
{
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private IMediator _mediator;

    public AssociateAccountToPaymentMethodConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountService accountService, IUserService userService, IMediator mediator) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService = userService;
        _mediator = mediator;
    }

    protected override async Task<long> Consume(AssociateAccountToPaymentMethod evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetByIdAsync(evt.CurrentUserId, cancellationToken)
            .ConfigureAwait(false);

        var paymentMethod = await session.GetAsync<PaymentMethod>(evt.PaymentMethodId, cancellationToken)
            .ConfigureAwait(false);

        if (paymentMethod == null)
        {
            throw new NotFoundException("Metodo di pagamento non trovato");
        }

        Models.Account targetAccount = await _accountService.GetById(evt.AccountId, currentUser, cancellationToken)
            .ConfigureAwait(false);

            
        if (paymentMethod == null)
        {
            throw new NotFoundException("Metodo di pagamento non trovato");
        }

        bool exists = await session.Query<AccountPaymentMethod>().Where(j => !j.IsDeleted)
            .Where(j => j.Account.Id == evt.AccountId && j.PaymentMethod.Id == evt.PaymentMethodId)
            .AnyAsync(cancellationToken).ConfigureAwait(false);

        if (exists)
        {
            throw new OxInvalidOperationException("Associazione già presente");
        }

        AccountPaymentMethod accountPayment = new AccountPaymentMethod()
        {
            Account = targetAccount, PaymentMethod = paymentMethod, IsDefault = false, IsDeleted = false,
            IsEnabled = true
        };

        accountPayment.Trace(currentUser);

        await session.SaveOrUpdateAsync(accountPayment, cancellationToken).ConfigureAwait(false);


        return accountPayment.Id;
    }
}