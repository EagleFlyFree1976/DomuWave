using DomuWave.Services.Models;

namespace DomuWave.Services.Models;

public class ChartOfAccountsCategory : TenantEntity<int>
{
    public virtual string  Name        { get; set; } = string.Empty;
    public virtual string? Description { get; set; }
    public virtual bool    IsActive    { get; set; } = true;

    public override int GetHashCode() => Id.GetHashCode();
}
