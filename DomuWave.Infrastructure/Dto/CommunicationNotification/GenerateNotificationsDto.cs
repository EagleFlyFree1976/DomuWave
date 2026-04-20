namespace DomuWave.Services.Dto.CommunicationNotification;

public class GenerateNotificationsDto
{
    public int  DeliveryMethod     { get; set; } = 0;   // 0=Email, 1=Raccomandata
    public int? NotificationTemplateId { get; set; }    // null = cerca default per tipo
    public DateTime? ScheduledAt   { get; set; }        // null = invia subito
}
