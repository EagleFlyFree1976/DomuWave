using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.Contabilita.FiscalYear
{
    /// <summary>
    /// DTO per le operazioni di transizione di stato dell'esercizio:
    /// StartClosing, Close, Lock.
    /// Contiene eventuali note dell'amministratore da registrare.
    /// </summary>
    public class FiscalYearCloseRequestDto
    {
        /// <summary>Note opzionali dell'amministratore da registrare nella transizione di stato.</summary>
        [MaxLength(500, ErrorMessage = "Le note non possono superare 500 caratteri.")]
        public string? Notes { get; set; }
    }
}
