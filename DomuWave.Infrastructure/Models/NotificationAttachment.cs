namespace DomuWave.Services.Models
{
    public class NotificationAttachment : TenantEntity<int>
    {
        public virtual CommunicationNotification Notification { get; set; } = null!;
        public virtual Document                  Document     { get; set; } = null!;

        public override int GetHashCode() => Id.GetHashCode();
    }
}
