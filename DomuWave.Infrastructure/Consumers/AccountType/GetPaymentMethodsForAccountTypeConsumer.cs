using DomuWave.Services.Command.Book;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using CPQ.Core.Consumers;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using NHibernate.Linq;
using SimpleMediator.Core;

namespace DomuWave.Services.Consumers.AccountType;

public class GetPaymentMethodsForAccountTypeConsumer : InMemoryConsumerBase<GetPaymentMethodsForAccountType,
    IList<(Models.PaymentMethod paymentMethod, bool IsDefault)>>
{
    private readonly IAccountTypeService _accountTypeService;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ICoreAuthorizationManager _authorizationManager;

    public GetPaymentMethodsForAccountTypeConsumer(ISessionFactoryProvider sessionFactoryProvider, IAccountTypeService accountTypeService, IUserService userService, IMediator mediator, ICoreAuthorizationManager authorizationManager) : base(sessionFactoryProvider)
    {
        _accountTypeService = accountTypeService;
        _userService = userService;
        _mediator = mediator;
        _authorizationManager = authorizationManager;
    }

    protected override async Task<IList<(PaymentMethod paymentMethod, bool IsDefault)>> Consume(GetPaymentMethodsForAccountType evt, IMediationContext mediationContext,
        CancellationToken cancellationToken)
    {
        var allAssociations = await session.Query<AccountTypePaymentMethod>()
            .GetQueryable<AccountTypePaymentMethod, int>().Where(k => k.AccountType.Id == evt.AccountTypeId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        IList<(PaymentMethod paymentMethod, bool IsDefault)> returnList =
            new List<(PaymentMethod paymentMethod, bool IsDefault)>();

        foreach (AccountTypePaymentMethod association in allAssociations)
        {
            returnList.Add((association.PaymentMethod, association.IsDefault));
        }

        return returnList;

    }
}