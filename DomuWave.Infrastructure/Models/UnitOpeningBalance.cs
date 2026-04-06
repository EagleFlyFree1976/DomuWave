using DomuWave.Services.Models;

namespace DomuWave.Services.Models;

/// <summary>
/// Bilancio (apertura + chiusura) di un'unità immobiliare per un esercizio.
///
/// Composizione del saldo:
///   OpeningBalance          = saldo riportato dall'esercizio precedente (morosità o credito pregresso)
///   RateAddebitate          = Σ CondominiumFee.AmountDue   (rate emesse nel preventivo)
///   RateIncassate           = Σ CondominiumFee.AmountPaid  (pagamenti ricevuti sulle rate)
///   InsolatoRate            = RateAddebitate - RateIncassate  (parte non pagata)
///   SaldoConguaglio         = quota consuntiva - rata preventivo  (+debito / -credito da conguaglio)
///   TotalMovements          = InsolatoRate + SaldoConguaglio  (movimenti netti dell'esercizio)
///   ClosingBalance          = OpeningBalance + TotalMovements (saldo da riportare al prossimo esercizio)
///
/// Regole di editabilità:
///   - OpeningBalance manualmente editabile solo sul primo esercizio del condominio.
///   - Negli esercizi successivi viene copiato automaticamente dal ClosingBalance precedente.
///   - TotalMovements e ClosingBalance vengono calcolati alla chiusura dell'esercizio.
///   - SaldoConguaglio viene impostato all'approvazione del Consuntivo.
/// </summary>
public class UnitOpeningBalance : TenantEntity<int>
{
    public virtual RealEstateUnit Unit       { get; set; }
    public virtual FiscalYear     FiscalYear { get; set; }

    /// <summary>
    /// Saldo di apertura riportato dall'esercizio precedente.
    /// Positivo = il condòmino è in credito; negativo = il condòmino è moroso.
    /// Modificabile manualmente solo sul primo esercizio.
    /// </summary>
    public virtual decimal OpeningBalance   { get; set; } = 0;

    // ── Dettaglio movimenti dell'esercizio ──────────────────────────────────

    /// <summary>Totale rate addebitate nell'esercizio (Σ CondominiumFee.AmountDue del Preventivo).</summary>
    public virtual decimal RateAddebitate   { get; set; } = 0;

    /// <summary>Totale rate incassate nell'esercizio (Σ CondominiumFee.AmountPaid del Preventivo).</summary>
    public virtual decimal RateIncassate    { get; set; } = 0;

    /// <summary>
    /// Quota consuntiva dell'unità = TotaleSpese * (Millesimi / TotMillesimi).
    /// Impostata all'approvazione del budget Consuntivo. Zero se il Consuntivo non è ancora approvato.
    /// </summary>
    public virtual decimal QuotaConsuntiva  { get; set; } = 0;

    /// <summary>
    /// Saldo di conguaglio = QuotaConsuntiva - RateAddebitate.
    /// Positivo = il condòmino deve un conguaglio a debito.
    /// Negativo = il condominio deve restituire un conguaglio a credito.
    /// Zero se il Consuntivo non è ancora approvato.
    /// </summary>
    public virtual decimal SaldoConguaglio  { get; set; } = 0;

    /// <summary>
    /// Totale movimenti netti = (RateAddebitate - RateIncassate) + SaldoConguaglio.
    /// Calcolato automaticamente alla chiusura dell'esercizio.
    /// </summary>
    public virtual decimal TotalMovements   { get; set; } = 0;

    /// <summary>
    /// Saldo di chiusura = OpeningBalance + TotalMovements.
    /// Positivo = morosità residua da riportare; negativo = credito da riportare.
    /// Calcolato alla chiusura e propagato come OpeningBalance del prossimo esercizio.
    /// </summary>
    public virtual decimal ClosingBalance   { get; set; } = 0;

    public virtual string  Notes            { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
