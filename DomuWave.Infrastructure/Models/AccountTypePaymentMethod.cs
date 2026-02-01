using CPQ.Core;

namespace DomuWave.Services.Models;

public class AccountTypePaymentMethod : TraceEntity<int>
{
    public virtual bool IsDefault { get; set; }
    public virtual AccountType AccountType { get; set; }
    public virtual PaymentMethod PaymentMethod { get; set; }

    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}