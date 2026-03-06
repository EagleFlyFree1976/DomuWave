namespace DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;

public class UpdateChartOfAccountsCategoryTemplateDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; } = true;
}
