namespace DomuWave.Services.Models;

public class ChartOfAccountsTemplateItem : TenantEntity<int>
{
    public virtual ChartOfAccountsTemplate  Template      { get; set; }
    public virtual ChartOfAccountsTemplateItem? ParentItem { get; set; }
    public virtual string               Code          { get; set; } = string.Empty;
    public virtual string               Name          { get; set; } = string.Empty;
    public virtual string?              Description   { get; set; }
    public virtual ChartOfAccountsType  Type          { get; set; } = ChartOfAccountsType.Uscita;
    public virtual int                  SortOrder     { get; set; }
    public virtual bool                 IsActive      { get; set; } = true;

    public override int GetHashCode() => Id.GetHashCode();
}
