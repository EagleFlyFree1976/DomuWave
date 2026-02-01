using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.Account;

public class GetPaymentMethodsForAccountConsumer : InMemoryConsumerBase<GetPaymentMethodsForAccountCommand,
    IList<(Models.PaymentMethod paymentMethod, bool IsDefault)>>
{
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;

    public GetPaymentMethodsForAccountConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountService accountService, IUserService userService, IMediator mediator, ICoreAuthorizationManager authorizationManager) : base(sessionFactoryProvider)
    {
        _accountService = accountService;
        _userService = userService;
        _mediator = mediator;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<IList<(PaymentMethod paymentMethod, bool IsDefault)>> Consume(GetPaymentMethodsForAccountCommand evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var allAssociations = await session.Query<AccountPaymentMethod>()
            .GetQueryable<AccountPaymentMethod, long>().Where(k => k.Account.Id == evt.AccountId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        IList<(PaymentMethod paymentMethod, bool IsDefault)> returnList =
            new List<(PaymentMethod paymentMethod, bool IsDefault)>();

        foreach (AccountPaymentMethod association in allAssociations)
        {
            returnList.Add((association.PaymentMethod, association.IsDefault));
        }

        return returnList;

    }
}