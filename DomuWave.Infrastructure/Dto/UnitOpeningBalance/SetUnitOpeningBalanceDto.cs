namespace DomuWave.Services.Dto.UnitOpeningBalance;

public class SetUnitOpeningBalanceDto
{
    public int     FiscalYearId   { get; set; }
    /// <summary>Saldo di apertura manuale (solo primo esercizio).</summary>
    public decimal OpeningBalance { get; set; }
    public string? Notes          { get; set; }
}
