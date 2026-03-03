using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Budget;

public class BudgetReadDto : TraceEntityDTO<int>
{
    public int CondominiumId { get; set; }
    public string? CondominiumName { get; set; }
    public int FiscalYearId { get; set; }
    public string? FiscalYearCode { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime? ApprovalDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public string? Notes { get; set; }
}
