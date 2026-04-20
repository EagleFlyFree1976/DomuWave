using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.CommunicationNotification;

public class CommunicationNotificationReadDto : TraceEntityDTO<long>
{
    public int     CommunicationId    { get; set; }
    public string? CommunicationTitle { get; set; }
    public long    RecipientUserId    { get; set; }
    public string? RecipientFullName  { get; set; }
    public int?    UnitId             { get; set; }
    public string? UnitDisplayName    { get; set; }
    public int     DeliveryMethod     { get; set; }
    public string  DeliveryMethodName { get; set; } = string.Empty;
    public int     Status             { get; set; }
    public string  StatusName         { get; set; } = string.Empty;
    public DateTime? ScheduledAt      { get; set; }
    public DateTime? SentAt           { get; set; }
    public DateTime? DeliveredAt      { get; set; }
    public DateTime? PrintedAt        { get; set; }
    public string? TrackingNumber     { get; set; }
    public string? ErrorMessage       { get; set; }
    public string? EmailAddress       { get; set; }
    public string? PostalAddress      { get; set; }
    public string? SubjectResolved    { get; set; }
    public string? BodyResolved       { get; set; }
}
