using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.ChartOfAccountsTemplate;

public class ChartOfAccountsTemplateItemReadDto
{
    public int                  Id           { get; set; }
    public int                  TemplateId   { get; set; }
    public int?                 ParentItemId { get; set; }
    public string               Code         { get; set; } = string.Empty;
    public string               Name         { get; set; } = string.Empty;
    public string?              Description  { get; set; }
    public ChartOfAccountsType  Type         { get; set; }
    public string               TypeLabel    { get; set; } = string.Empty;
    public int                  SortOrder    { get; set; }
    public bool                 IsActive     { get; set; }
}
