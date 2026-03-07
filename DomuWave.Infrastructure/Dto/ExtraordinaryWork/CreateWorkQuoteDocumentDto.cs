using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.ExtraordinaryWork;

public class CreateWorkQuoteDocumentDto
{
    [Required]
    public int QuoteId { get; set; }

    [Required]
    public string Title    { get; set; } = string.Empty;

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public long   FileSize     { get; set; }
    public string MimeType     { get; set; } = "application/octet-stream";
    public string? DocumentType { get; set; }
    public string? Notes        { get; set; }
}
