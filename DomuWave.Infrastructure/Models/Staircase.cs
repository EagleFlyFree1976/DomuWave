namespace DomuWave.Services.Models;

public class Staircase : TenantEntity<int>
{
    public virtual Condominium Condominium { get; set; }
    public virtual Building?   Building   { get; set; }
    public virtual bool        IsActive   { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
}
