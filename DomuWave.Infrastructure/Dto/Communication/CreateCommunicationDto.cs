namespace DomuWave.Services.Dto.Communication;

public class CreateCommunicationDto
{
    public int     CondominiumId     { get; set; }
    public int?    InstallmentId     { get; set; }
    public string  Title             { get; set; } = string.Empty;
    public string  Content           { get; set; } = string.Empty;
    public string  CommunicationType { get; set; } = string.Empty;
    public string? Priority          { get; set; }
    public DateTime  PublicationDate { get; set; }
    public DateTime? ExpirationDate  { get; set; }
    public bool    SendEmail         { get; set; }
    public bool    IsVisible         { get; set; }
    public string? AttachmentPath    { get; set; }
}
