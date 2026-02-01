using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;
using DomuWave.Services.Helper;

namespace DomuWave.Services.Models;

public class Transaction : AccountEntity<long>
{
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }

    /// <summary>
    ///  Se la transazione è un trasferimento tra due account, questo campo identifica l'account di destinazione/provenienza    
    /// </summary>
    public virtual Account DestinationAccount { get; set; }

    public virtual decimal Amount { get; set; }



    /// <summary>
    /// Tasso di cambio riferituo tra la valuta della transazione e la valuta del book di rifetimento
    /// </summary>
    public virtual decimal? BookCurrencyExchangeRate { get; set; }


    /// <summary>
    ///  Amount expresso della valuta del book di riferimento,
    ///  valore calcolato come Amount * BookCurrencyExchangeRate
    /// </summary>
    public virtual decimal AmountInBookCurrency
    {
        get;
        protected set;
    }


    /// <summary>
    /// Tasso di cambio riferituo tra la valuta della transazione e la valuta dell'account di rifetimento
    /// </summary>
    public virtual decimal? AccountCurrencyExchangeRate { get; set; }


    /// <summary>
    ///  Amount expresso nella valuta dell'account di riferimento,
    ///  valore calcolato come Amount * AccountCurrencyExchangeRate
    /// </summary>
    public virtual decimal AmountInAccountCurrency
    {
        get;
        protected set;
    }

    public virtual Currency Currency { get; set; }
    public virtual Beneficiary Beneficiary { get; set; }

    public virtual TransactionStatus Status { get; set; }

    /// <summary>
    ///  Metodo di pagamento utilizzato
    /// </summary>
    public virtual PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    ///  Categoria di appartenenza della transazione
    /// </summary>
    public virtual Category Category { get; set; }

 

    public virtual IList<TransactionTag> Tags { get; set; } = new List<TransactionTag>();

    public virtual DateTime TransactionDate { get; set; }
    
    [NotMapped]
    public virtual TransactionType TransactionType
    {
        get => TransactionTypeMap.GetEnum(TransactionTypeCode);
        set => TransactionTypeCode = TransactionTypeMap.GetCode(value);
    }

    
                                                                   
    [NotMapped]
    public virtual FlowDirection FlowDirection
    {
        get => FlowDirectionMap.GetEnum(FlowDirectionCode);
        set => FlowDirectionCode = FlowDirectionMap.GetCode(value);
    }

    public virtual string TransactionTypeCode { get; set; } = "U"; // campo reale nel DB
    public virtual string FlowDirectionCode { get; set; } = "O"; // campo reale nel DB
    
    public virtual Guid? TransferKey { get; set; }



    /// <summary>
    ///     saldo dell'account dopo l'applicazione della transazione
    /// </summary>
    public virtual decimal? AccountBalance { get; set; }

    /// <summary>
    ///     saldo complessivo dopo l'applicazione della transazione
    /// </summary>
    public virtual decimal? BookBalance { get; set; }
}