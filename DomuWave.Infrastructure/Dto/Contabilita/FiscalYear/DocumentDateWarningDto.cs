using System;

namespace DomuWave.Services.Dto.Contabilita.FiscalYear
{
    /// <summary>
    /// Risposta al controllo di competenza della data documento.
    /// Indica se la data documento cade fuori dal range dell'esercizio selezionato
    /// e suggerisce l'esercizio corretto per competenza economica.
    /// </summary>
    public class DocumentDateWarningDto
    {
        /// <summary>True se la data documento è fuori dal range dell'esercizio selezionato.</summary>
        public bool IsOutOfRange { get; set; }

        /// <summary>La data documento sottoposta al controllo.</summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>Data di inizio dell'esercizio selezionato.</summary>
        public DateTime FiscalYearStart { get; set; }

        /// <summary>Data di fine dell'esercizio selezionato.</summary>
        public DateTime FiscalYearEnd { get; set; }

        /// <summary>
        /// Esercizio suggerito per competenza economica.
        /// Null se non viene trovato un esercizio corrispondente o se IsOutOfRange = false.
        /// </summary>
        public FiscalYearListItemDto? SuggestedFiscalYear { get; set; }

        /// <summary>
        /// Messaggio di warning localizzato da mostrare all'utente.
        /// Null se IsOutOfRange = false.
        /// </summary>
        public string? WarningMessage { get; set; }
    }
}
