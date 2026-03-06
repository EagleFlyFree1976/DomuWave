namespace DomuWave.Services.Dto.ChartOfAccountsCategory;

public class ChartOfAccountsCategoryReadDto
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; }
}
