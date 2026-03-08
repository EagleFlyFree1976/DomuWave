using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.ChartOfAccountsTemplate;

/// <summary>Used for both create and update of template items.</summary>
public class SaveChartOfAccountsTemplateItemDto
{
    public int                  TemplateId   { get; set; }
    public int?                 ParentItemId { get; set; }
    public string               Code         { get; set; } = string.Empty;
    public string               Name         { get; set; } = string.Empty;
    public string?              Description  { get; set; }
    public ChartOfAccountsType  Type         { get; set; } = ChartOfAccountsType.Uscita;
    public int                  SortOrder    { get; set; }
    public bool                 IsActive     { get; set; } = true;
}
