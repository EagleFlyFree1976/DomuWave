using System.Collections.Generic;
using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class Fault : TenantEntity<int>
    {
        public virtual Condominium Condominium { get; set; } = null!;
        public virtual RealEstateUnit? Unit { get; set; }
        public virtual FaultStatus Status { get; set; } = null!;
        public virtual long ReporterUserId { get; set; }
        public virtual string ReporterFullName { get; set; } = string.Empty;

        public virtual IList<FaultMessage> Messages { get; set; } = new List<FaultMessage>();

        public override int GetHashCode() => Id.GetHashCode();
    }
}
