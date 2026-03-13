using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Budget;

public class BudgetItemReadDto : TraceEntityDTO<int>
{
    public int     BudgetId          { get; set; }
    public int     AccountId         { get; set; }
    public string? AccountCode       { get; set; }
    public string? AccountName       { get; set; }
    public string? AccountType       { get; set; }
    public int     AccountLevel      { get; set; }
    public int?    ParentAccountId   { get; set; }
    public string? ParentAccountCode { get; set; }
    public string? ParentAccountName { get; set; }
    public string? Description       { get; set; }
    public decimal Amount            { get; set; }
    public string? Notes             { get; set; }
}
