using System.ComponentModel.DataAnnotations.Schema;
using DomuWave.Services.Helper;
using CPQ.Core;

namespace DomuWave.Services.Models.Import;

public class ImportTransaction : TraceEntity<long>
{
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }

    public virtual Import Import { get; set; }

    public virtual string InTransactionDate { get; set; }
    public virtual string InDepositAmount { get; set; }
    public virtual string InWithdrawalAmount { get; set; }
    public virtual string InAmount { get; set; }

    public virtual string InCategoryName { get; set; }
    public virtual string InSubCategoryName { get; set; }
    public virtual string Description { get; set; }
    public virtual string InCurrency { get; set; }

    public virtual string InBeneficiary { get; set; }
    public virtual string InType { get; set; }
    public virtual string InStatus { get; set; }


    public virtual decimal? DepositAmount { get; set; }
    public virtual decimal? WithdrawalAmount { get; set; }
    public virtual decimal? Amount { get; set; }



    public virtual DateTime TransactionDate { get; set; }
    
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


    public virtual ImportTransactionStatus ImportTransactionStatus { get; set; } = ImportTransactionStatus.New;

    public virtual string ValidateErrors { get; set; }
}


public enum ImportTransactionStatus
{
    New,
    Imported,
    Validated,
    Ignored,
    Error
}