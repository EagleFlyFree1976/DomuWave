using CPQ.Core;

namespace DomuWave.Services.Models;

public class ExchangeRateHistory : TraceEntity<long>
{
    public virtual Currency FromCurrency { get; set; }
    public virtual Currency ToCurrency { get; set; }

    public virtual decimal Rate { get; set; }

    public virtual DateTime ValidFrom { get; set; }
    public virtual DateTime? ValidTo { get; set; }



    public override int GetHashCode()
    {
        
        return this.Id.GetHashCode();
    }
}