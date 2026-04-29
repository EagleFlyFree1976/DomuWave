namespace DomuWave.Services.Dto.CommunicationNotification;

public class GenerateNotificationsFromFeesDto
{
    /// <summary>Se null, viene creata una nuova Communication automaticamente.</summary>
    public int?   CommunicationId       { get; set; }

    /// <summary>ID delle rate selezionate (almeno una).</summary>
    public IList<int> InstallmentIds    { get; set; } = [];

    /// <summary>ID delle unità a cui inviare. Null = tutte le unità delle rate selezionate.</summary>
    public IList<long>? UnitIds         { get; set; }

    public int    DeliveryMethod        { get; set; } = 0;   // 0=Email, 1=Raccomandata
    public int?   NotificationTemplateId { get; set; }
    public DateTime? ScheduledAt        { get; set; }
}
