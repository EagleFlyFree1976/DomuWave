using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
///  Associa il metodo di pagamento all'account specificato
/// </summary>
public class AssociateAccountToPaymentMethod : BaseBookRelatedCommand, IQuery<long>
{
    public long AccountId { get; set; }
    public int PaymentMethodId { get; set; }

    public AssociateAccountToPaymentMethod()
    {
    }

    public AssociateAccountToPaymentMethod(long accountId, int paymentMethodId, int currentUserId, long bookId) : base(currentUserId, bookId)
    {
        AccountId = accountId;
        PaymentMethodId = paymentMethodId;
    }
}