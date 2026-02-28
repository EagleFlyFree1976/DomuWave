using CPQ.Core;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models;

public interface IownerEntity
{
      int OwnerId { get; set; }
}


public abstract class TenantEntity<T> : GenericEntity<T>
{
    public virtual Tenant Tenant { get; set; } 
}
public abstract class CondominiumEntity<T> : TraceEntity<T>
{
    public virtual Condominium Condominium { get; set; } 
}

