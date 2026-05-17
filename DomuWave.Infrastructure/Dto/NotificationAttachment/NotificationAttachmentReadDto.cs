using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.NotificationAttachment;

public class NotificationAttachmentReadDto : TraceEntityDTO<int>
{
    public long   NotificationId { get; set; }
    public int    DocumentId     { get; set; }
    public string DocumentTitle  { get; set; } = string.Empty;
    public string FileName       { get; set; } = string.Empty;
    public string? MimeType      { get; set; }
    public long   FileSize       { get; set; }
    public string? Category      { get; set; }
}
