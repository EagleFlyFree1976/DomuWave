

namespace DomuWave.Services.Models;

public class Book : GenericEntity<long>,IownerEntity
{
    public virtual Currency Currency { get; set; }
    public virtual int OwnerId { get; set; }
    public virtual bool IsPrimary { get; set; }
    public virtual bool IsSystem { get; set; }
    public virtual bool IsArchived { get; set; }
    public override int GetHashCode()
    {
            
        return this.Id.GetHashCode();
    }

    
}