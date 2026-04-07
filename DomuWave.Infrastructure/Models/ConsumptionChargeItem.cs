namespace DomuWave.Services.Models;

public class ConsumptionChargeItem : TenantEntity<int>
{
    public virtual ConsumptionCharge Charge           { get; set; }
    public virtual RealEstateUnit    Unit             { get; set; }
    public virtual decimal           Consumption      { get; set; } = 0;
    public virtual decimal           TotalConsumption { get; set; } = 0;
    public virtual decimal           Percentage       { get; set; } = 0;
    public virtual decimal           Amount           { get; set; } = 0;
    /// <summary>True se l'unità non ha letture — importo forzato a zero con warning.</summary>
    public virtual bool              HasWarning       { get; set; } = false;

    public override int GetHashCode() => Id.GetHashCode();
}
