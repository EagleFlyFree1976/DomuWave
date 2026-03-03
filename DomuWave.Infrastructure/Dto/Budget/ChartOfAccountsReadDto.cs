namespace DomuWave.Services.Dto.Budget;

public class ChartOfAccountsReadDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public int? ParentAccountId { get; set; }
}
