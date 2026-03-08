namespace DomuWave.Services.Dto.ChartOfAccountsTemplate;

public class CreateChartOfAccountsTemplateDto
{
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; } = true;
}
