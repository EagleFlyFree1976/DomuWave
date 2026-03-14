using DomuWave.Services.Models;

namespace DomuWave.Services.Models;

/// <summary>
/// Bilancio (apertura + chiusura) di un'unità immobiliare per un esercizio.
/// - OpeningBalance: saldo iniziale. Modificabile manualmente solo sul primo esercizio
///   o quando non esiste un esercizio precedente chiuso.
///   Negli esercizi successivi viene copiato automaticamente dal ClosingBalance precedente.
/// - TotalMovements: somma algebrica dei movimenti dell'esercizio (quote addebitate - pagamenti ricevuti).
///   Calcolato automaticamente alla chiusura dell'esercizio.
/// - ClosingBalance: OpeningBalance + TotalMovements. Calcolato alla chiusura.
/// </summary>
public class UnitOpeningBalance : TenantEntity<int>
{
    public virtual RealEstateUnit Unit       { get; set; }
    public virtual FiscalYear     FiscalYear { get; set; }

    /// <summary>Saldo di apertura. Positivo = credito verso il condominio; negativo = debito.</summary>
    public virtual decimal OpeningBalance  { get; set; } = 0;

    /// <summary>
    /// Totale movimenti dell'esercizio: quote addebitate (CondominiumFee.AmountDue)
    /// meno pagamenti ricevuti (CondominiumFee.AmountPaid). Calcolato alla chiusura.
    /// </summary>
    public virtual decimal TotalMovements  { get; set; } = 0;

    /// <summary>Saldo di chiusura = OpeningBalance + TotalMovements. Calcolato alla chiusura.</summary>
    public virtual decimal ClosingBalance  { get; set; } = 0;

    public virtual string  Notes          { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
