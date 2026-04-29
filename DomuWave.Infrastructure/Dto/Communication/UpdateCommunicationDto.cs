namespace DomuWave.Services.Dto.Communication;

public class UpdateCommunicationDto
{
    public string?   Title             { get; set; }
    public string?   Content           { get; set; }
    public string?   CommunicationType { get; set; }
    public string?   Priority          { get; set; }
    public DateTime? PublicationDate   { get; set; }
    public DateTime? ExpirationDate    { get; set; }
    public bool?     SendEmail         { get; set; }
    public bool?     IsVisible         { get; set; }
    public string?   AttachmentPath    { get; set; }
}
