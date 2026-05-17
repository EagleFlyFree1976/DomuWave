using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class FaultMessage : TenantEntity<int>
    {
        public virtual Fault Fault { get; set; } = null!;
        public virtual long AuthorUserId { get; set; }
        public virtual string AuthorFullName { get; set; } = string.Empty;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
