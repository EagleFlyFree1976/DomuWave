namespace DomuWave.Services.Models;

public class ConsumptionCharge : TenantEntity<int>
{
    public virtual ConsumptionType         ConsumptionType { get; set; }
    public virtual FiscalYear              FiscalYear      { get; set; }
    public virtual Budget                  Budget          { get; set; }
    public virtual Expense                 Expense         { get; set; }
    public virtual decimal                 TotalAmount     { get; set; } = 0;
    public virtual ConsumptionChargeStatus Status          { get; set; }
    public virtual string                  Notes           { get; set; }

    public virtual IList<ConsumptionChargeItem> Items { get; set; } = new List<ConsumptionChargeItem>();

    public override int GetHashCode() => Id.GetHashCode();
}
