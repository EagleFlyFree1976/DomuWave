using System;

namespace DomuWave.Services.Dto.Contabilita.FiscalYear
{
    /// <summary>
    /// DTO di lettura completo per un esercizio condominiale.
    /// Include dati anagrafici, stato, date di transizione, audit e riepilogo finanziario opzionale.
    /// </summary>
    public class FiscalYearReadDto
    {
        public int Id { get; set; }
        public Guid TenantId { get; set; }
        public int CondominiumId { get; set; }
        public string CondominiumName { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>1=Draft | 2=Open | 3=Closing | 4=Closed | 5=Locked</summary>
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public DateTime? ClosingDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public DateTime? LockedDate { get; set; }
        public string? ClosingNotes { get; set; }

        /// <summary>ID dell'esercizio di provenienza (null per il primo esercizio).</summary>
        public int? PreviousFiscalYearId { get; set; }
        /// <summary>Codice dell'esercizio di provenienza (null per il primo esercizio).</summary>
        public string? PreviousFiscalYearCode { get; set; }

        // Audit
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string? CreatedByFullName { get; set; }
        public string? LastUpdatedByFullName { get; set; }

        /// <summary>
        /// Riepilogo finanziario aggregato dell'esercizio (calcolato su richiesta).
        /// Null nella lista, popolato nel dettaglio.
        /// </summary>
        public FiscalYearSummaryDto? Summary { get; set; }
    }
}
