namespace DomuWave.Services.Dto.UnitOpeningBalance;

public class SetUnitOpeningBalancesBulkDto
{
    public int FiscalYearId { get; set; }
    public IList<UnitOpeningBalanceItemDto> Items { get; set; } = [];
    /// <summary>Righe di saldo per gruppo di fatturazione (unità con gruppo non compaiono in Items).</summary>
    public IList<GroupOpeningBalanceItemDto> GroupItems { get; set; } = [];
}

public class UnitOpeningBalanceItemDto
{
    public int     UnitId         { get; set; }
    public decimal OpeningBalance { get; set; }
    public string? Notes          { get; set; }
}

public class GroupOpeningBalanceItemDto
{
    public int     BillingGroupId { get; set; }
    public decimal OpeningBalance { get; set; }
    public string? Notes          { get; set; }
}
