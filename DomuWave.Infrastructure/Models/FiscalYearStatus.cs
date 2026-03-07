namespace DomuWave.Services.Models
{
    /// <summary>
    /// Lookup entity per gli stati del ciclo di vita di un FiscalYear.
    /// </summary>
    public class FiscalYearStatus
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; }

        // ─── Costanti ID ───────────────────────────────────────────
        /// <summary>Esercizio creato ma non ancora aperto (default alla creazione).</summary>
        public const int Draft   = 1;

        /// <summary>Esercizio aperto: registrazioni libere.</summary>
        public const int Open    = 2;

        /// <summary>Esercizio in fase di chiusura: ancora aperto a rettifiche.</summary>
        public const int Closing = 3;

        /// <summary>Esercizio chiuso definitivamente. IsActive = false.</summary>
        public const int Closed  = 4;

        /// <summary>Esercizio bloccato. Nessuna modifica consentita.</summary>
        public const int Locked  = 5;
    }
}
