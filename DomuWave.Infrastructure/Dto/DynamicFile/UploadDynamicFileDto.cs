using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.DynamicFile;

public class UploadDynamicFileDto
{
    /// <summary>Nome completo del tipo entita' (es. "DomuWave.Services.Models.Expense")</summary>
    [Required]
    public string EntityFullName { get; set; }

    /// <summary>Nome semplice del tipo entita' (es. "Expense")</summary>
    [Required]
    public string EntityName { get; set; }

    /// <summary>Id dell'entita' a cui il file e' collegato</summary>
    [Required]
    public int EntityId { get; set; }

    [Required]
    public string FileName { get; set; }

    /// <summary>MIME type (es. "application/pdf")</summary>
    [Required]
    public string ContentType { get; set; }

    /// <summary>Contenuto del file in base64</summary>
    [Required]
    public string Base64Data { get; set; }

    public string? Description { get; set; }

    public string? Tags { get; set; }
}
