namespace DomuWave.Services.Dto.Budget;

public class CreateBudgetItemDto
{
    public int BudgetId { get; set; }
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
