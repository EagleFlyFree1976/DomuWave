namespace DomuWave.Services.Dto.NotificationTemplate;

public class CreateNotificationTemplateDto
{
    public int    CondominiumId     { get; set; }
    public string CommunicationType { get; set; } = string.Empty;
    public string Name              { get; set; } = string.Empty;
    public string SubjectTemplate   { get; set; } = string.Empty;
    public string BodyTemplate      { get; set; } = string.Empty;
    public bool   IsDefault         { get; set; }
}
