namespace DomuWave.Services.Dto.Contabilita.FiscalYear;

/// <summary>
/// Report "Bilancio di ripartizione": una riga per i proprietari e una per gli inquilini
/// (se presenti) di ogni unità, con colonne dinamiche per tipo di consumo e tabella
/// millesimale, più spese dirette all'immobile e accrediti diretti.
/// </summary>
public class BilancioRipartizioneReportDto
{
    public int     FiscalYearId    { get; set; }
    public string? FiscalYearCode  { get; set; }
    public int     CondominiumId   { get; set; }
    public string? CondominiumName { get; set; }

    /// <summary>Colonne dei tipi di consumo usati (in ordine).</summary>
    public List<ConsumoColumnDto>     ConsumoColumns     { get; set; } = new();
    /// <summary>Colonne delle tabelle millesimali usate (in ordine).</summary>
    public List<MillesimaleColumnDto> MillesimaleColumns { get; set; } = new();

    public List<BilancioRowDto> Rows { get; set; } = new();

    /// <summary>Riga dei totali di colonna.</summary>
    public BilancioTotalsDto Totals { get; set; } = new();
}

public class ConsumoColumnDto
{
    public int     ConsumptionTypeId { get; set; }
    public string  Name              { get; set; } = string.Empty;
    public string? UnitOfMeasure     { get; set; }
}

public class MillesimaleColumnDto
{
    public int    MillesimalTableId { get; set; }
    public string Name              { get; set; } = string.Empty;
}

public class BilancioRowDto
{
    public int     UnitId    { get; set; }
    /// <summary>Etichetta scala/gruppo (facoltativa).</summary>
    public string? Scala     { get; set; }
    /// <summary>Nominativo: unione dei cognomi (proprietari oppure inquilini).</summary>
    public string  Nominativo { get; set; } = string.Empty;
    /// <summary>"Proprietari" | "Inquilini".</summary>
    public string  RowType   { get; set; } = "Proprietari";

    /// <summary>Valori per colonna consumo (chiave = ConsumptionTypeId).</summary>
    public List<ConsumoCellDto> Consumi { get; set; } = new();
    /// <summary>Importo ripartito per colonna millesimale (chiave = MillesimalTableId).</summary>
    public List<MillesimaleCellDto> Millesimali { get; set; } = new();

    /// <summary>Spese imputate direttamente all'immobile (uscite, segno positivo = a carico).</summary>
    public decimal Dirette   { get; set; }
    /// <summary>Accrediti all'unità = entrate dirette al singolo immobile + entrate ripartite (millesimali).</summary>
    public decimal Accrediti { get; set; }

    /// <summary>Totale speso a carico della riga = consumi + millesimali + dirette - accrediti.</summary>
    public decimal Totale    { get; set; }

    /// <summary>Versato dall'unità = quote condominiali effettivamente incassate (CondominiumFee.AmountPaid).</summary>
    public decimal Versato   { get; set; }

    /// <summary>Saldo = versato - totale speso (positivo = credito unità, negativo = da versare).</summary>
    public decimal Saldo     { get; set; }
}

public class ConsumoCellDto
{
    public int     ConsumptionTypeId { get; set; }
    public decimal Quantita          { get; set; }
    public decimal Importo           { get; set; }
}

public class MillesimaleCellDto
{
    public int     MillesimalTableId { get; set; }
    /// <summary>Valore millesimale dell'unità in questa tabella.</summary>
    public decimal Millesimal        { get; set; }
    public decimal Importo           { get; set; }
}

public class BilancioTotalsDto
{
    public List<ConsumoCellDto>     Consumi     { get; set; } = new();
    public List<MillesimaleCellDto> Millesimali { get; set; } = new();
    public decimal Dirette   { get; set; }
    public decimal Accrediti { get; set; }
    public decimal Totale    { get; set; }
    public decimal Versato   { get; set; }
    public decimal Saldo     { get; set; }
}
