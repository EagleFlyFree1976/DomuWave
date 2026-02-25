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

        /// <summary>Open | Closing | Closed | Locked</summary>
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
