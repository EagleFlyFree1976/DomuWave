using CPQ.Core.DTO;

namespace DomuWave.Services.Models.Dto.Transaction;

public class TransactionMassiveUpdateDto
{
    public bool UpdateAccountId { get; set; } = false;
    public bool UpdateDescription { get; set; } = false;
    public bool UpdateStatus { get; set; } = false;
    public bool UpdateAmount { get; set; } = false;
    public bool UpdateCurrencyId { get; set; } = false;
    public bool UpdateBeneficiary { get; set; } = false;
    public bool UpdatePaymentMethodId { get; set; } = false;
    public bool UpdateCategoryId { get; set; } = false;
    public bool UpdateTransactionDate { get; set; } = false;
    public bool UpdateTransactionType { get; set; } = false;
    public bool UpdateDestinationAccountId { get; set; } = false;

    public long? AccountId { get; set; }
    
    public string? Description { get; set; }

    public int? Status { get; set; }

    public decimal? Amount { get; set; }

    /// <summary>
    ///  la valuta del movimento, se null recupero quella dell'account
    /// </summary>
    public int? CurrencyId { get; set; }


    public LookupEntityDto<long>? Beneficiary { get; set; }

    /// <summary>
    ///  Metodo di pagamento utilizzato, se null prendo quello di default per l'account
    /// </summary>
    public int? PaymentMethodId { get; set; }

    /// <summary>
    ///  Categoria di appartenenza della transazione
    /// </summary>
    public long? CategoryId { get; set; }




    /// <summary>
    ///  la data della transazione, se null prendo now
    /// </summary>
    public DateTime? TransactionDate { get; set; }


    public TransactionType? TransactionType { get; set; }



    public long? DestinationAccountId { get; set; }

}