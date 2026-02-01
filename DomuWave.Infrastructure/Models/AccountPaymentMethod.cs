using CPQ.Core;

namespace DomuWave.Services.Models;


/// <summary>
///  questa classe permette di legare ad un account piu metodi di pagamento
///  es. Account paypal
///         metodo di pagamento
///                 conto corrente webank
///                 carta di credito
///                 saldo
///                 ecc
///     Conto Corrente Webank
///             Bonifico
///             SSD
///             Carta di debito
///     ecc.
/// </summary>
public class AccountPaymentMethod : TraceEntity<long>
{
    public virtual bool IsDefault { get; set; }
    public virtual bool IsEnabled { get; set; }
    public virtual Account Account { get; set; }
    public virtual PaymentMethod PaymentMethod { get; set; }

    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}