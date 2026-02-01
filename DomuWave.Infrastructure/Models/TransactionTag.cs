using CPQ.Core;

namespace DomuWave.Services.Models;

public class TransactionTag : TraceEntity<long>
{

    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
    public virtual Transaction Transaction { get; set; }
    public virtual Tag Tag { get; set; }

}