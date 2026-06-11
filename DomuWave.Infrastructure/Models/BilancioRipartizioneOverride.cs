namespace DomuWave.Services.Models;

/// <summary>
/// Tipo di riga del bilancio di ripartizione a cui si riferisce un override manuale.
/// </summary>
public static class BilancioRowType
{
    public const int Proprietari = 0;
    public const int Inquilini   = 1;
}

/// <summary>
/// Tipo di cella sovrascrivibile in una riga del bilancio di ripartizione.
/// Per Consumo/Millesimale <see cref="BilancioRipartizioneOverride.ColumnRefId"/> identifica
/// la colonna (ConsumptionTypeId o MillesimalTableId); per le altre celle è 0.
/// </summary>
public static class BilancioCellType
{
    public const int Consumo     = 0;
    public const int Millesimale = 1;
    public const int Dirette     = 2;
    public const int Accrediti   = 3;
    public const int Versato     = 4;
}

/// <summary>
/// Override manuale di una singola cella del bilancio di ripartizione di un esercizio.
/// Il bilancio è calcolato al volo: questi record sostituiscono il valore calcolato per
/// la cella identificata da (FiscalYear, Unit, RowType, CellType, ColumnRefId).
/// </summary>
public class BilancioRipartizioneOverride : TenantEntity<int>
{
    public virtual FiscalYear     FiscalYear  { get; set; } = null!;
    public virtual RealEstateUnit Unit        { get; set; } = null!;

    /// <summary><see cref="BilancioRowType"/> — 0 = Proprietari, 1 = Inquilini.</summary>
    public virtual int RowType { get; set; }

    /// <summary><see cref="BilancioCellType"/>.</summary>
    public virtual int CellType { get; set; }

    /// <summary>ConsumptionTypeId o MillesimalTableId per consumi/millesimali; 0 altrimenti.</summary>
    public virtual int ColumnRefId { get; set; }

    /// <summary>Valore manuale che sostituisce l'importo calcolato della cella.</summary>
    public virtual decimal Amount { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
