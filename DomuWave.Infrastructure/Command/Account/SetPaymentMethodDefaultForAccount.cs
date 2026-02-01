using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class SetPaymentMethodDefaultForAccount : BaseBookRelatedCommand, IQuery<bool>
{
    public long AccountId { get; set; }
    public int PaymentMethodId { get; set; }

    public SetPaymentMethodDefaultForAccount()
    {
    }

    public SetPaymentMethodDefaultForAccount(long accountId, int paymentMethodId, int currentUserId, long bookId) : base(currentUserId, bookId)
    {
        AccountId = accountId;
        PaymentMethodId = paymentMethodId;
    }
}