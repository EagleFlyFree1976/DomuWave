using NHibernate.Type;

namespace DomuWave.Services.Models;

public class Account : BookEntity<long>, IownerEntity
{

    public virtual int OwnerId { get; set; }
    public virtual AccountType AccountType { get; set; }

    public virtual Currency Currency { get; set; }

    public virtual bool IsActive { get; set; }
    public virtual decimal InitialBalance { get; set; }
    public virtual decimal Balance { get; set; }

    public virtual DateTime OpenDate { get; set; }

    public virtual DateTime? ClosedDate { get; set; }

 
    
    public override int GetHashCode()
    {
 
        return this.Id.GetHashCode();
         
    }
}