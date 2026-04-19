using System;
using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    /// <summary>
    /// Rappresenta una rata condominiale, ovvero un pagamento periodico
    /// che i condomini devono versare nell'ambito di un piano di rateizzazione
    /// delle spese condominiali.
    /// Ogni rata è legata a un condominio, a un budget approvato e a un anno fiscale.
    /// </summary>
    public class CondominiumInstallment : TenantEntity<int>
    {
        /// <summary>Il condominio a cui appartiene questa rata.</summary>
        public virtual Condominium Condominium { get; set; } = null!;

        /// <summary>
        /// Il budget/preventivo di riferimento da cui è stata generata la rata.
        /// Può essere null se la rata è stata creata manualmente.
        /// </summary>
        public virtual Budget? Budget { get; set; }

        /// <summary>L'anno fiscale di competenza della rata.</summary>
        public virtual FiscalYear FiscalYear { get; set; } = null!;

        /// <summary>
        /// Numero progressivo della rata all'interno del piano di rateizzazione
        /// (es. 1 = prima rata, 2 = seconda rata, ecc.).
        /// </summary>
        public virtual int InstallmentNumber { get; set; }

        /// <summary>Data di scadenza entro cui il pagamento deve essere effettuato.</summary>
        public virtual DateTime DueDate { get; set; }

        /// <summary>Importo totale della rata, somma di tutte le quote delle unità immobiliari.</summary>
        public virtual decimal TotalAmount { get; set; }

        /// <summary>
        /// Stato corrente della rata (FK a CondominiumInstallmentStatusLookup).
        /// Costanti: CondominiumInstallmentStatus.Draft | Open | Paid | Overdue | Cancelled.
        /// </summary>
        public virtual CondominiumInstallmentStatus Status { get; set; }

        /// <summary>Note libere aggiuntive sulla rata.</summary>
        public virtual string? Notes { get; set; }

        /// <summary>
        /// Elenco delle singole quote (CondominiumFee) associate a questa rata,
        /// una per ciascuna unità immobiliare del condominio.
        /// La somma degli importi delle fee corrisponde al TotalAmount della rata.
        /// </summary>
        public virtual IList<CondominiumFee> Fees { get; set; } = new List<CondominiumFee>();

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
