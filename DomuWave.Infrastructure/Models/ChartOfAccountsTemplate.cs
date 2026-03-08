using System.Collections.Generic;

namespace DomuWave.Services.Models;

public class ChartOfAccountsTemplate : TenantEntity<int>
{
    public virtual string  Name        { get; set; } = string.Empty;
    public virtual string? Description { get; set; }
    public virtual bool    IsActive    { get; set; } = true;

    public virtual IList<ChartOfAccountsTemplateItem> Items { get; set; } = new List<ChartOfAccountsTemplateItem>();

    public override int GetHashCode() => Id.GetHashCode();
}
