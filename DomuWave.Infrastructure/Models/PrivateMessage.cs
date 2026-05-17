using DomuWave.Services.Models;

namespace DomuWave.Services.Models
{
    public class PrivateMessage : TenantEntity<int>
    {
        public virtual PrivateThread Thread { get; set; } = null!;
        public virtual long SenderUserId { get; set; }
        public virtual string SenderFullName { get; set; } = string.Empty;
        public virtual bool IsReadByRecipient { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
