using CPQ.Core.DTO;
using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.Budget;

public class BudgetReadDto : TraceEntityDTO<int>
{
    public Guid TenantId { get; set; }
    public int CondominiumId { get; set; }
    public string? CondominiumName { get; set; }
    public int FiscalYearId { get; set; }
    public string? FiscalYearCode { get; set; }
    public BudgetType Type { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public int    StatusId   { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public string? Notes { get; set; }
}
