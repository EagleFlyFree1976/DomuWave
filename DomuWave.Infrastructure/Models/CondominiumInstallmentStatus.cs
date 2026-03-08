namespace DomuWave.Services.Models
{
    /// <summary>
    /// Lookup entity per gli stati del ciclo di vita di una CondominiumInstallment.
    /// I valori sono fissi e non modificabili (tabella CondominiumInstallmentStatusLookup).
    /// </summary>
    public class CondominiumInstallmentStatus
    {
        public virtual int    Id   { get; set; }
        public virtual string Name { get; set; }

        // ─── Costanti ID ───────────────────────────────────────────
        /// <summary>Rata creata ma non ancora emessa ai condomini.</summary>
        public const int Draft = 1;

        /// <summary>Rata emessa e in attesa di pagamento.</summary>
        public const int Open = 2;

        /// <summary>Rata pagata integralmente.</summary>
        public const int Paid = 3;

        /// <summary>Rata scaduta senza pagamento completo.</summary>
        public const int Overdue = 4;

        /// <summary>Rata annullata.</summary>
        public const int Cancelled = 5;
    }
}
