namespace DomuWave.Services.Dto.UnitOpeningBalance;

public class SetGroupOpeningBalanceDto
{
    public int     FiscalYearId { get; set; }
    public decimal TotalAmount  { get; set; }
    public string? Notes        { get; set; }

    /// <summary>Criterio di ripartizione: "millesimal" (default, per millesimi) oppure "equal" (parti uguali).</summary>
    public string  Criterion    { get; set; } = "millesimal";
}
