using DomuWave.Services.Models;

namespace DomuWave.Services.Models;

/// <summary>
/// Incasso registrato nel periodo di un esercizio fiscale.
/// Rappresenta un versamento effettivo ricevuto da un condomino,
/// opzionalmente collegato a una quota (CondominiumFee) per la riconciliazione.
/// Ciclo: Pending → Reconciled | Disputed.
/// </summary>
public class Payment : TenantEntity<long>
{
    /// <summary>Esercizio fiscale di competenza.</summary>
    public virtual FiscalYear FiscalYear { get; set; } = null!;

    /// <summary>Condominio di riferimento.</summary>
    public virtual Condominium Condominium { get; set; } = null!;

    /// <summary>Unità immobiliare del versante.</summary>
    public virtual RealEstateUnit Unit { get; set; } = null!;

    /// <summary>
    /// Quota condominiale a cui il pagamento è imputato.
    /// Nullable: il pagamento può essere registrato prima della riconciliazione.
    /// </summary>
    public virtual CondominiumFee? Fee { get; set; }

    /// <summary>ID dell'utente (owner) che ha effettuato il versamento.</summary>
    public virtual long UserId { get; set; }

    /// <summary>Data effettiva del versamento.</summary>
    public virtual DateTime PaymentDate { get; set; }

    /// <summary>Data di registrazione nel sistema.</summary>
    public virtual DateTime RegistrationDate { get; set; }

    /// <summary>Importo versato.</summary>
    public virtual decimal Amount { get; set; }

    /// <summary>Metodo di pagamento (Bonifico, Contanti, RID, ecc.).</summary>
    public virtual string? PaymentMethod { get; set; }

    /// <summary>Riferimento bancario per la riconciliazione (CRO, TRN, ecc.).</summary>
    public virtual string? BankReference { get; set; }

    /// <summary>
    /// Stato di riconciliazione.
    /// Valori: Pending | Reconciled | Disputed.
    /// </summary>
    public virtual string? ReconciliationStatus { get; set; }

    /// <summary>Note libere sull'incasso.</summary>
    public virtual string? Notes { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}