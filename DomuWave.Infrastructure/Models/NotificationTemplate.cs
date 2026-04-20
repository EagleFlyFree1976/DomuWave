using System.Collections.Generic;

namespace DomuWave.Services.Models
{
    public class NotificationTemplate : TenantEntity<int>
    {
        public virtual Condominium Condominium        { get; set; } = null!;
        public virtual string      CommunicationType  { get; set; } = string.Empty;
        public virtual string      SubjectTemplate    { get; set; } = string.Empty;
        public virtual string      BodyTemplate       { get; set; } = string.Empty;
        public virtual bool        IsDefault          { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
