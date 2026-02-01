using DomuWave.Services.Models.Dto.Category;
using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto.Transaction;

public class TransactionReadDto : BookEntityDto<long>
{
    public LookupEntityDto<long> Account { get; set; }
    public LookupEntityDto<long> DestinationAccount { get; set; }

    public decimal Amount { get; set; }
    public decimal AmountInAccountCurrency { get; set; }

    
    public LookupEntityDtoExtended<int> Status { get; set; }

    /// <summary>
    ///  la valuta del movimento, se null recupero quella dell'account
    /// </summary>
    public LookupEntityDto<int> Currency { get; set; }
    public LookupEntityDto<int> AccountCurrency { get; set; }

    /// <summary>
    ///  Metodo di pagamento utilizzato, se null prendo quello di default per l'account
    /// </summary>
    public LookupEntityDto<int> PaymentMethod { get; set; }
    public LookupEntityDto<long> Beneficiary { get; set; }


    /// <summary>
    ///  Categoria di appartenenza della transazione
    /// </summary>
    public CategoryMinReadDto Category { get; set; }



    public virtual IList<string> Tags { get; set; } = new List<string>();

    /// <summary>
    ///  la data della transazione, se null prendo now
    /// </summary>
    public virtual DateTime? TransactionDate { get; set; }


    public TransactionType TransactionType
    {
        get; set;
    }
    //    AccountCurrencyExchangeRate { get; set; }
    public decimal? Rate { get; set; }


    public virtual FlowDirection FlowDirection
    {
        get; set;
    }


    public decimal? AccountBalance { get; set; }
}