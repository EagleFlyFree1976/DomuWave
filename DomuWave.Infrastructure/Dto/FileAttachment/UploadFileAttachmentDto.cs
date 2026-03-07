using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.FileAttachment;

public class UploadFileAttachmentDto
{
    [Required]
    public string EntityKey { get; set; }

    [Required]
    public int EntityId { get; set; }

    [Required]
    public string FileName { get; set; }

    [Required]
    public string ContentType { get; set; }

    /// <summary>Contenuto del file in base64</summary>
    [Required]
    public string Base64Data { get; set; }
}
