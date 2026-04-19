namespace DomuWave.Services.Models;

public class ConsumptionReading : TenantEntity<int>
{
    public virtual Meter      Meter        { get; set; } = null!;
    public virtual FiscalYear FiscalYear   { get; set; } = null!;
    public virtual DateTime?  InitialDate  { get; set; }
    public virtual decimal    InitialValue { get; set; } = 0;
    public virtual DateTime?  FinalDate    { get; set; }
    public virtual decimal    FinalValue   { get; set; } = 0;
    /// <summary>Colonna calcolata persistita: FinalValue - InitialValue.</summary>
    public virtual decimal    Consumption  { get; set; } = 0;
    public virtual string?            Notes   { get; set; }
    /// <summary>Ripartizione a cui questa lettura è stata assegnata. NULL = non ancora ripartita.</summary>
    public virtual ConsumptionCharge? Charge { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
