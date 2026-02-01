using DomuWave.Services.Models;
using CPQ.Core.DTO;

namespace DomuWave.Application.Models;

public   class TransactionCreateUpdateDto
{
    public   long AccountId { get; set; }

    public   long? DestinationAccountId { get; set; }

    public string Description { get; set; }

    public int Status { get; set; }

    public decimal Amount { get; set; }

    /// <summary>
    ///  la valuta del movimento, se null recupero quella dell'account
    /// </summary>
    public int? CurrencyId { get; set; }


    public LookupEntityDto<long> Beneficiary { get; set; }

    /// <summary>
    ///  Metodo di pagamento utilizzato, se null prendo quello di default per l'account
    /// </summary>
    public int? PaymentMethodId { get; set; }

    /// <summary>
    ///  Categoria di appartenenza della transazione
    /// </summary>
    public long CategoryId { get; set; }



    public IList<string> Tags { get; set; } = new List<string>();

    /// <summary>
    ///  la data della transazione, se null prendo now
    /// </summary>
    public DateTime? TransactionDate { get; set; }


    public TransactionType TransactionType
    {
        get;
        set;

    }



 

}