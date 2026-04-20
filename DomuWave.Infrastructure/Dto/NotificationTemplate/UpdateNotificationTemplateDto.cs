namespace DomuWave.Services.Dto.NotificationTemplate;

public class UpdateNotificationTemplateDto
{
    public string? Name             { get; set; }
    public string? SubjectTemplate  { get; set; }
    public string? BodyTemplate     { get; set; }
    public bool?   IsDefault        { get; set; }
}
