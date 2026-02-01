using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
///  Imposto il metodo di pagamento selezionato di default per la tipologia di account specificata
/// </summary>
public class SetPaymentMethodDefaultForAccountType : BaseCommand, IQuery<bool>
{
    public int AccountTypeId { get; set; }
    public int PaymentMethodId { get; set; }

    public SetPaymentMethodDefaultForAccountType()
    {
    }

    public SetPaymentMethodDefaultForAccountType(int accountTypeId, int paymentMethodId)
    {
        AccountTypeId = accountTypeId;
        PaymentMethodId = paymentMethodId;
    }

    public SetPaymentMethodDefaultForAccountType(int accountTypeId, int paymentMethodId, int currentUserId) : base(currentUserId)
    {
        AccountTypeId = accountTypeId;
        PaymentMethodId = paymentMethodId;
    }
}