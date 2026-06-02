namespace DomuWave.Services.Dto.Contabilita.FiscalYear;

/// <summary>
/// Report: elenco delle spese di un esercizio fiscale raggruppato per tabella millesimale.
/// Le spese imputate direttamente a un immobile sono raggruppate a parte.
/// </summary>
public class ExpensesByMillesimalReportDto
{
    public int      FiscalYearId   { get; set; }
    public string?  FiscalYearCode { get; set; }
    public int      CondominiumId  { get; set; }
    public string?  CondominiumName { get; set; }

    public List<ExpensesByMillesimalGroupDto> Groups { get; set; } = new();

    /// <summary>Totale complessivo di tutte le spese del report.</summary>
    public decimal  GrandTotal     { get; set; }
}

public class ExpensesByMillesimalGroupDto
{
    /// <summary>Id tabella millesimale; null per il gruppo "Imputazione diretta a immobile".</summary>
    public int?     MillesimalTableId   { get; set; }
    /// <summary>Etichetta del gruppo (nome tabella o "Imputazione diretta a immobile").</summary>
    public string   GroupName           { get; set; } = string.Empty;
    /// <summary>True se è il gruppo delle spese imputate a singolo immobile.</summary>
    public bool     IsDirectUnit        { get; set; }

    public List<ExpenseReportRowDto> Expenses { get; set; } = new();

    public decimal  GroupTotal          { get; set; }
}

public class ExpenseReportRowDto
{
    public long      ExpenseId      { get; set; }
    public DateTime? DocumentDate   { get; set; }
    public string?   DocumentNumber { get; set; }
    public string    Name           { get; set; } = string.Empty;
    public string?   SupplierName   { get; set; }
    public string?   AccountName    { get; set; }
    /// <summary>Valorizzato solo per le righe del gruppo "Imputazione diretta a immobile".</summary>
    public string?   UnitName       { get; set; }
    public decimal   GrossAmount    { get; set; }
}
