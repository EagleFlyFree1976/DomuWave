namespace DomuWave.Services.Dto.UnitOpeningBalance;

public class SetUnitOpeningBalancesBulkDto
{
    public int FiscalYearId { get; set; }
    public IList<UnitOpeningBalanceItemDto> Items { get; set; } = [];
}

public class UnitOpeningBalanceItemDto
{
    public int     UnitId         { get; set; }
    public decimal OpeningBalance { get; set; }
    public string? Notes          { get; set; }
}
