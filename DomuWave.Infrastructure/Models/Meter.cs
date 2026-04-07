namespace DomuWave.Services.Models;

public class Meter : TenantEntity<int>
{
    public virtual ConsumptionType ConsumptionType  { get; set; }
    public virtual RealEstateUnit  Unit             { get; set; }
    public virtual string          Code             { get; set; }
    public virtual string          Notes            { get; set; }
    public virtual bool            IsActive         { get; set; } = true;

    public virtual IList<ConsumptionReading> Readings { get; set; } = new List<ConsumptionReading>();

    public override int GetHashCode() => Id.GetHashCode();
}
