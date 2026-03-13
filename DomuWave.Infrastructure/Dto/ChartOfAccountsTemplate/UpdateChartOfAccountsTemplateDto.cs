namespace DomuWave.Services.Dto.ChartOfAccountsTemplate;

public class UpdateChartOfAccountsTemplateDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; } = true;
    public bool    IsDefault   { get; set; } = false;
}
