using CPQ.Core;

namespace DomuWave.Services.Models;

public class Currency : TraceEntity<int>
{
    public virtual string Code { get; set; }
    public virtual string Symbol { get; set; }
    public virtual string Name { get; set; }
    public virtual int DecimalDigits { get; set; }

    public virtual bool IsDefault { get; set; }



    public override int GetHashCode()
    {
        
        return this.Id.GetHashCode();
    }
    
}