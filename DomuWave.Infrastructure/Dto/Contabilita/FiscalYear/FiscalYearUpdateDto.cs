using System;
using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.Contabilita.FiscalYear
{
    /// <summary>
    /// DTO per l'aggiornamento di un esercizio in stato Open.
    /// Non è possibile modificare esercizi in stato Closing, Closed o Locked.
    /// </summary>
    public class FiscalYearUpdateDto
    {
        /// <summary>
        /// Descrizione libera. Aggiornabile in stato Open e Closing.
        /// </summary>
        [MaxLength(200, ErrorMessage = "La descrizione non può superare 200 caratteri.")]
        public string? Description { get; set; }

        /// <summary>
        /// Nuova data di fine esercizio.
        /// Modificabile solo se Status = Open.
        /// Deve essere successiva a StartDate e non causare sovrapposizioni.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
