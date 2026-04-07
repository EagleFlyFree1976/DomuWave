namespace DomuWave.Services.Models;

public class ConsumptionChargeStatus
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int Draft    = 1;
    public const int Approved = 2;
}
