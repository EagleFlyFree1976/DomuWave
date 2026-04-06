using System;
using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.Contabilita.FiscalYear
{
    /// <summary>
    /// DTO per la creazione di un nuovo esercizio condominiale.
    /// </summary>
    public class FiscalYearCreateDto
    {
        /// <summary>Identificativo del condominio.</summary>
        [Required(ErrorMessage = "Il condominio è obbligatorio.")]
        public int CondominiumId { get; set; }

        /// <summary>Codice identificativo dell'esercizio (es. "2024-2025", "ES-001").</summary>
        [Required(ErrorMessage = "Il codice è obbligatorio.")]
        [MaxLength(50, ErrorMessage = "Il codice non può superare 50 caratteri.")]
        public string Code { get; set; } = string.Empty;

        /// <summary>Descrizione libera dell'esercizio.</summary>
        [MaxLength(200, ErrorMessage = "La descrizione non può superare 200 caratteri.")]
        public string? Description { get; set; }

        /// <summary>Data di inizio del periodo di esercizio.</summary>
        [Required(ErrorMessage = "La data di inizio è obbligatoria.")]
        public DateTime StartDate { get; set; }

        /// <summary>Data di fine del periodo di esercizio. Deve essere successiva a StartDate.</summary>
        [Required(ErrorMessage = "La data di fine è obbligatoria.")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// ID dell'esercizio di provenienza selezionato dall'utente.
        /// Null per il primo esercizio del condominio.
        /// </summary>
        public int? PreviousFiscalYearId { get; set; }
    }
}
