using System;

namespace DomuWave.Services.Models
{
    public class CommunicationNotification : TenantEntity<long>
    {
        public virtual Communication      Communication     { get; set; } = null!;
        public virtual long               RecipientUserId   { get; set; }
        public virtual string?            RecipientFullName { get; set; }
        public virtual RealEstateUnit?    Unit              { get; set; }
        public virtual int                DeliveryMethod    { get; set; }   // 0=Email, 1=Raccomandata
        public virtual int                Status            { get; set; }   // 0=Draft,1=Scheduled,2=Sent,3=Delivered,4=Failed,5=Printed
        public virtual DateTime?          ScheduledAt       { get; set; }
        public virtual DateTime?          SentAt            { get; set; }
        public virtual DateTime?          DeliveredAt       { get; set; }
        public virtual DateTime?          PrintedAt         { get; set; }
        public virtual string?            TrackingNumber    { get; set; }
        public virtual string?            ErrorMessage      { get; set; }
        public virtual string?            EmailAddress      { get; set; }
        public virtual string?            PostalAddress     { get; set; }
        public virtual string?            SubjectResolved   { get; set; }
        public virtual string?            BodyResolved      { get; set; }
        public virtual string?            UnitsDisplay      { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
    }
}
