namespace DomuWave.Services.Dto.Budget;

// ── Vista per conto ───────────────────────────────────────────────────────

public class ConsuntivoDetailDto
{
    public int                              BudgetId       { get; set; }
    public string?                          FiscalYear     { get; set; }
    /// <summary>True se esistono ExpenseAllocation — abilita la vista per unità.</summary>
    public bool                             HasAllocations { get; set; }
    public List<ConsuntivoAccountRowDto>    Accounts       { get; set; } = [];
    public List<ConsuntivoUnitRowDto>       Units          { get; set; } = [];
}

public class ConsuntivoAccountRowDto
{
    public int                              AccountId    { get; set; }
    public string?                          AccountCode  { get; set; }
    public string?                          AccountName  { get; set; }
    public int                              Level        { get; set; }
    public int?                             ParentId     { get; set; }
    public decimal                          TotalAmount  { get; set; }
    public List<ConsuntivoExpenseRowDto>    Expenses     { get; set; } = [];
}

public class ConsuntivoExpenseRowDto
{
    public long                                 ExpenseId       { get; set; }
    public string?                              Name            { get; set; }
    public decimal                              GrossAmount     { get; set; }
    public string?                              SupplierName    { get; set; }
    public DateTime                             DocumentDate    { get; set; }
    public List<ConsuntivoAllocationRowDto>     Allocations     { get; set; } = [];
}

public class ConsuntivoAllocationRowDto
{
    public int      UnitId          { get; set; }
    public string?  UnitName        { get; set; }
    public decimal  Millesimal      { get; set; }
    public decimal  AllocatedAmount { get; set; }
}

// ── Vista per unità ───────────────────────────────────────────────────────

public class ConsuntivoUnitRowDto
{
    public int                                  UnitId       { get; set; }
    public string?                              UnitName     { get; set; }
    public decimal                              Total        { get; set; }
    public List<ConsuntivoUnitAccountEntryDto>  Entries      { get; set; } = [];
}

public class ConsuntivoUnitAccountEntryDto
{
    public int      AccountId       { get; set; }
    public string?  AccountCode     { get; set; }
    public string?  AccountName     { get; set; }
    public decimal  AllocatedAmount { get; set; }
}
