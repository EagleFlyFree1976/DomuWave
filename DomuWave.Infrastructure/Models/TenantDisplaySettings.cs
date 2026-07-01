namespace DomuWave.Services.Models
{
    /// <summary>
    /// Convenzione di visualizzazione del segno per le voci contabili.
    /// </summary>
    public enum AccountingSignConvention
    {
        /// <summary>Magnitudine + colore (comportamento storico): le uscite sono in rosso senza segno meno.</summary>
        SoloColore = 0,

        /// <summary>Segno esplicito: le uscite hanno il segno meno, le entrate il segno più.</summary>
        SegnoEsplicito = 1,
    }

    /// <summary>
    /// Impostazioni di visualizzazione per tenant (formattazione dei valori contabili).
    /// Un solo record attivo per tenant.
    /// </summary>
    public class TenantDisplaySettings : TenantEntity<int>
    {
        public virtual AccountingSignConvention AccountingSignConvention { get; set; }
            = AccountingSignConvention.SoloColore;

        // ── Logo del tenant (usato in sidebar e nei report esportati) ──────────────
        /// <summary>Contenuto binario del logo (VARBINARY(MAX)). Null se non impostato.</summary>
        public virtual byte[] LogoContent { get; set; }

        /// <summary>Content-type MIME del logo (es. image/png).</summary>
        public virtual string LogoContentType { get; set; }

        /// <summary>Nome file originale del logo caricato.</summary>
        public virtual string LogoFileName { get; set; }

        /// <summary>Data ultimo aggiornamento del logo (usata per cache-busting).</summary>
        public virtual DateTime? LogoUpdatedDate { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
