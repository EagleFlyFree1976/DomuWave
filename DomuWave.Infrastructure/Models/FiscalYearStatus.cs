namespace DomuWave.Services.Models
{
    /// <summary>
    /// Costanti per gli stati del ciclo di vita di un FiscalYear.
    /// </summary>
    public static class FiscalYearStatus
    {
        /// <summary>Esercizio aperto: registrazioni libere.</summary>
        public const string Open = "Open";

        /// <summary>
        /// Esercizio in fase di chiusura: ancora aperto a registrazioni di rettifica
        /// (es. fatture con data documento nel periodo ma ricevute dopo la chiusura formale).
        /// </summary>
        public const string Closing = "Closing";

        /// <summary>
        /// Esercizio chiuso definitivamente. IsActive = false.
        /// È possibile aprire un nuovo esercizio.
        /// </summary>
        public const string Closed = "Closed";

        /// <summary>
        /// Esercizio bloccato. Nessuna modifica consentita.
        /// Tipicamente dopo l'approvazione del consuntivo in assemblea.
        /// </summary>
        public const string Locked = "Locked";
    }
}
