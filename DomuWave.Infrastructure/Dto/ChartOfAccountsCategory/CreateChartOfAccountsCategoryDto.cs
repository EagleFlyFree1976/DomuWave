namespace DomuWave.Services.Dto.ChartOfAccountsCategory;

public class CreateChartOfAccountsCategoryDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; } = true;
}
