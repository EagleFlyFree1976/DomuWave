using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
///  ritorna l'elenenco di metodi di pagamento disponibili per l'accounttype specificato
/// </summary>
public class GetPaymentMethodsForAccountType : BaseCommand, IQuery<IList<(Models.PaymentMethod paymentMethod, bool IsDefault)>>
{
    public int AccountTypeId { get; set; }

    public GetPaymentMethodsForAccountType()
    {
    }

    public GetPaymentMethodsForAccountType(int accountTypeId, int currentUserId) : base(currentUserId)
    {
        AccountTypeId = accountTypeId;
    }
}