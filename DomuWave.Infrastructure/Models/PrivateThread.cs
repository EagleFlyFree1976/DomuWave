using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class PrivateThread : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; } = null!;
        public virtual long CondominioUserId { get; set; }

        public virtual IList<PrivateMessage> Messages { get; set; } = new List<PrivateMessage>();

        public override int GetHashCode() => Id.GetHashCode();
    }
}
