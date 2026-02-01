using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class GetPaymentMethodsForAccountCommand : BaseCommand, IQuery<IList<(Models.PaymentMethod paymentMethod, bool IsDefault)>>
{
    public long AccountId { get; set; }

    public GetPaymentMethodsForAccountCommand()
    {
    }

    public GetPaymentMethodsForAccountCommand(long accountId, int currentUserId) : base(currentUserId)
    {
        AccountId = accountId;
    }
}