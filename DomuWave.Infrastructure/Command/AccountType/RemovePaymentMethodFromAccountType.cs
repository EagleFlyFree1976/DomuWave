using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
///  Rimuove l'associazione del metodo di pagamento  dalla tipologia di conto
/// </summary>
public class RemovePaymentMethodFromAccountType : BaseCommand, IQuery<bool>
{
    public int AccountTypeId { get; set; }
    public int PaymentMethodId { get; set; }
}