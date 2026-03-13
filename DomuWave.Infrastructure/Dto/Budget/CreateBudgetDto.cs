using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.Budget;

public class CreateBudgetDto
{
    public int CondominiumId { get; set; }
    public int FiscalYearId { get; set; }
    public BudgetType Type { get; set; } = BudgetType.Preventivo;
    public string? Notes { get; set; }
}
