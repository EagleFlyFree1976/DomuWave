namespace DomuWave.Services.Dto.UnitOpeningBalance;

public class SetGroupOpeningBalanceDto
{
    public int     FiscalYearId   { get; set; }
    /// <summary>Saldo di apertura manuale del gruppo (solo primo esercizio). Non viene ripartito sulle unità componenti.</summary>
    public decimal OpeningBalance { get; set; }
    public string? Notes          { get; set; }
}
