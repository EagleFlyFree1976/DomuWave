namespace DomuWave.Services.Dto.ChartOfAccountsTemplate;

public class ChartOfAccountsTemplateReadDto
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; }
    public int     ItemCount   { get; set; }
}
