using System;

namespace DomuWave.Services.Dto.Contabilita.FiscalYear
{
    /// <summary>
    /// DTO compatto per le liste di esercizi (elenchi, dropdown, selezioni).
    /// </summary>
    public class FiscalYearListItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>1=Draft | 2=Open | 3=Closing | 4=Closed | 5=Locked</summary>
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        /// <summary>ID dell'esercizio di provenienza (null per il primo esercizio).</summary>
        public int? PreviousFiscalYearId { get; set; }
        /// <summary>Codice dell'esercizio di provenienza (null per il primo esercizio).</summary>
        public string? PreviousFiscalYearCode { get; set; }
    }
}
