using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;


/// <summary>
/// Associa l'accountType al metodo di pagamento
/// </summary>
public class AssociatePaymentMethodToAccountType : BaseCommand, IQuery<int>
{
    public int AccountTypeId { get; set; }
    public int PaymentMethodId { get; set; }
    public bool IsDefault { get; set; }

    public AssociatePaymentMethodToAccountType()
    {
    }

    public AssociatePaymentMethodToAccountType(int accountTypeId, int paymentMethodId, bool isDefault, int currentUserId) : base(currentUserId)
    {
        AccountTypeId = accountTypeId;
        PaymentMethodId = paymentMethodId;
        IsDefault = isDefault;
    }
}