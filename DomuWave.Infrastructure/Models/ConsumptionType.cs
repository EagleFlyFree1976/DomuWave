namespace DomuWave.Services.Models;

public class ConsumptionType : TenantEntity<int>
{
    public virtual Condominium     Condominium   { get; set; } = null!;
    public virtual ChartOfAccounts? Account     { get; set; }
    public virtual string          UnitOfMeasure { get; set; } = string.Empty;
    public virtual string?         Notes         { get; set; }
    public virtual bool           IsActive      { get; set; } = true;

    public virtual IList<Meter> Meters { get; set; } = new List<Meter>();

    public override int GetHashCode() => Id.GetHashCode();
}
