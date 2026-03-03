namespace DomuWave.Services.Dto.Budget;

public class CreateBudgetDto
{
    public int CondominiumId { get; set; }
    public int FiscalYearId { get; set; }
    public string Type { get; set; } = "Preventivo";
    public decimal TotalIncome { get; set; }
    public string? Notes { get; set; }
}
