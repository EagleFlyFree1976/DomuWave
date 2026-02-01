using CPQ.Core;

namespace DomuWave.Services.Models;

public class AccountType : LookupEntity<int>
{
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }
}