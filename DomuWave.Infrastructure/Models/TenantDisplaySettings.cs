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

        public override int GetHashCode() => Id.GetHashCode();
    }
}
