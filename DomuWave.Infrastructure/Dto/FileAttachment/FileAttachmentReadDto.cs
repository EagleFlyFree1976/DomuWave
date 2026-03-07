namespace DomuWave.Services.Dto.FileAttachment;

public class FileAttachmentReadDto
{
    public int Id { get; set; }
    public string EntityKey { get; set; }
    public int EntityId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Valorizzato solo quando richiesto esplicitamente (download)</summary>
    public string Base64Data { get; set; }
}
