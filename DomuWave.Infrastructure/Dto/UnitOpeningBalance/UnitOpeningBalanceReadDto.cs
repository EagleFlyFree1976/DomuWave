using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.UnitOpeningBalance;

public class UnitOpeningBalanceReadDto : TraceEntityDTO<int>
{
    public int     UnitId         { get; set; }
    public string  UnitName       { get; set; }
    public int     FiscalYearId   { get; set; }
    public string  FiscalYearCode { get; set; }

    /// <summary>
    /// True se la riga rappresenta un GRUPPO di fatturazione (aggregato di più unità) invece di
    /// una singola unità. Quando true, UnitId è 0 e il saldo va gestito/salvato sul gruppo:
    /// non viene mai spalmato sulle unità componenti.
    /// </summary>
    public bool    IsGroup          { get; set; }

    /// <summary>Gruppo di fatturazione a cui appartiene l'unità (o riga di gruppo stessa), se presente.</summary>
    public int?    BillingGroupId   { get; set; }
    public string? BillingGroupName { get; set; }

    /// <summary>Millesimi dell'unità (o somma millesimi del gruppo) nella tabella millesimale principale del condominio (0 se non assegnati).</summary>
    public decimal Millesimal       { get; set; }

    /// <summary>Saldo riportato dall'esercizio precedente (morosità o credito pregresso).</summary>
    public decimal OpeningBalance  { get; set; }

    // ── Dettaglio movimenti dell'esercizio ──────────────────────────────────

    /// <summary>Totale rate addebitate nell'esercizio (Σ CondominiumFee.AmountDue del Preventivo).</summary>
    public decimal RateAddebitate  { get; set; }

    /// <summary>Totale rate incassate nell'esercizio (Σ CondominiumFee.AmountPaid del Preventivo).</summary>
    public decimal RateIncassate   { get; set; }

    /// <summary>Insoluto rate = RateAddebitate - RateIncassate.</summary>
    public decimal InsolatoRate    => RateAddebitate - RateIncassate;

    /// <summary>Quota consuntiva spettante all'unità (impostata all'approvazione del Consuntivo).</summary>
    public decimal QuotaConsuntiva { get; set; }

    /// <summary>
    /// Saldo conguaglio = QuotaConsuntiva - RateAddebitate.
    /// Positivo = conguaglio a debito; negativo = conguaglio a credito.
    /// </summary>
    public decimal SaldoConguaglio { get; set; }

    /// <summary>
    /// Totale movimenti netti = InsolatoRate + SaldoConguaglio.
    /// Calcolato definitivamente alla chiusura dell'esercizio.
    /// </summary>
    public decimal TotalMovements  { get; set; }

    /// <summary>
    /// Saldo di chiusura = OpeningBalance + TotalMovements.
    /// Viene propagato come OpeningBalance del prossimo esercizio (riporto morosità).
    /// </summary>
    public decimal ClosingBalance  { get; set; }

    public string  Notes           { get; set; }

    /// <summary>True se il saldo di apertura è modificabile manualmente (solo primo esercizio senza precedente chiuso).</summary>
    public bool    IsEditable      { get; set; }

    /// <summary>True se l'esercizio è stato chiuso (ClosingBalance e TotalMovements sono definitivi).</summary>
    public bool    IsClosed        { get; set; }
}
