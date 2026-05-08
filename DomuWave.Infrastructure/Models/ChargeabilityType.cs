namespace DomuWave.Services.Models;

public class ChargeabilityType
{
    public virtual int    Id   { get; set; }
    public virtual string Name { get; set; }

    public const int Owner  = 1;
    public const int Tenant = 2;
    public const int Auto   = 3;
}
