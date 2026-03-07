using System;
using System.Collections.Generic;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    /// <summary>
    /// Esercizio condominiale: contenitore temporale per tutti i movimenti finanziari
    /// (spese, rate, incassi) di un determinato periodo.
    /// Ciclo di vita: Open → Closing → Closed → Locked.
    /// Un solo esercizio può avere IsActive = true per condominio alla volta.
    /// Vincolo DB: UQ_FiscalYear_ActivePerCondominium (indice filtrato su IsActive = 1).
    /// </summary>
    public class FiscalYear : TenantEntity<int>
    {
        /// <summary>Condominio di appartenenza.</summary>
        public virtual Condominium Condominium { get; set; }

        /// <summary>Codice identificativo dell'esercizio (es. "2024-2025", "ES-001").</summary>
        public virtual string Code { get; set; }

        /// <summary>Descrizione libera dell'esercizio.</summary>
        public virtual string Description { get; set; }

        /// <summary>Data di inizio del periodo di esercizio.</summary>
        public virtual DateTime StartDate { get; set; }

        /// <summary>Data di fine del periodo di esercizio. Deve essere successiva a StartDate.</summary>
        public virtual DateTime EndDate { get; set; }

        /// <summary>
        /// Stato corrente dell'esercizio (FK a FiscalYearStatusLookup).
        /// Costanti: FiscalYearStatus.Draft | Open | Closing | Closed | Locked.
        /// </summary>
        public virtual FiscalYearStatus Status { get; set; }

        /// <summary>
        /// Indica se l'esercizio è quello attualmente attivo per il condominio.
        /// Un solo esercizio può essere attivo per volta (vincolo DB su indice filtrato).
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>Data in cui è iniziata la fase di chiusura (Status = Closing).</summary>
        public virtual DateTime? ClosingDate { get; set; }

        /// <summary>Data di chiusura definitiva (Status = Closed). IsActive passa a false.</summary>
        public virtual DateTime? ClosedDate { get; set; }

        /// <summary>Data di blocco definitivo (Status = Locked). Nessuna modifica consentita.</summary>
        public virtual DateTime? LockedDate { get; set; }

        /// <summary>ID dell'utente che ha eseguito la chiusura.</summary>
        public virtual int? ClosedByUserId { get; set; }

        /// <summary>Note dell'amministratore registrate in fase di chiusura.</summary>
        public virtual string ClosingNotes { get; set; }

        /// <summary>Bilanci (preventivo e consuntivo) associati all'esercizio. Lazy loading.</summary>
        public virtual IList<Budget> Budgets { get; set; } = new List<Budget>();

        /// <summary>Spese registrate nel periodo dell'esercizio. Lazy loading.</summary>
        public virtual IList<Expense> Expenses { get; set; } = new List<Expense>();

        /// <summary>Rate condominiali generate nel periodo dell'esercizio. Lazy loading.</summary>
        public virtual IList<CondominiumInstallment> Installments { get; set; } = new List<CondominiumInstallment>();

        /// <summary>Incassi registrati nel periodo dell'esercizio. Lazy loading.</summary>
        public virtual IList<Payment> Payments { get; set; } = new List<Payment>();

        public override int GetHashCode() => Id.GetHashCode();
    }
}


 