namespace DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;

public class ChartOfAccountsCategoryTemplateReadDto
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; }
}
