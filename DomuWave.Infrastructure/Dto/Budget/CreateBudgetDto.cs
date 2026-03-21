using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.Budget;

public class CreateBudgetDto
{
    public int        CondominiumId          { get; set; }
    public int        FiscalYearId           { get; set; }
    public BudgetType Type                   { get; set; } = BudgetType.Preventivo;
    public string?    Notes                  { get; set; }

    /// <summary>
    /// (Opzionale) ID del budget Consuntivo dell'esercizio precedente da cui importare le voci.
    /// Se valorizzato, le voci vengono copiate applicando IncreasePercentage.
    /// </summary>
    public int?    SourceConsuntivoId    { get; set; }

    /// <summary>
    /// Percentuale di maggiorazione da applicare agli importi copiati dal consuntivo precedente.
    /// Es: 5 = +5%. Default 0 (nessuna maggiorazione).
    /// </summary>
    public decimal IncreasePercentage   { get; set; } = 0;
}
