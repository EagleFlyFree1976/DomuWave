namespace DomuWave.Services.Dto.Document;

public class UploadDocumentDto
{
    public int     CondominiumId     { get; set; }
    public string  Title             { get; set; } = string.Empty;
    public string? Category          { get; set; }
    public string  FileName          { get; set; } = string.Empty;
    public string? MimeType          { get; set; }
    public long    FileSize          { get; set; }
    public DateTime? DocumentDate    { get; set; }
    public bool    IsVisibleToOwners { get; set; }
    public string? Tags              { get; set; }
    public string? Description       { get; set; }
    public byte[]  FileContent       { get; set; } = Array.Empty<byte>();
}
