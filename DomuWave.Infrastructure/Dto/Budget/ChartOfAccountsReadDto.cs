using DomuWave.Services.Models;

namespace DomuWave.Services.Dto.Budget;

public class ChartOfAccountsReadDto
{
    public int                  Id              { get; set; }
    public string               Code            { get; set; } = string.Empty;
    public string               Name            { get; set; } = string.Empty;
    public ChartOfAccountsType  Type            { get; set; }
    public string?              Category        { get; set; }
    public int                  Level           { get; set; }
    public bool                 IsActive        { get; set; }
    public int?                 ParentAccountId { get; set; }
}
