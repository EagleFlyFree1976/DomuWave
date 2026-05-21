using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Document;

public class DocumentReadDto : TraceEntityDTO<int>
{
    public int     CondominiumId      { get; set; }
    public string  CondominiumName    { get; set; } = string.Empty;
    public string  Title              { get; set; } = string.Empty;
    public string? Category           { get; set; }
    public string  FileName           { get; set; } = string.Empty;
    public long    FileSize           { get; set; }
    public string? MimeType           { get; set; }
    public DateTime? DocumentDate     { get; set; }
    public bool    IsVisibleToOwners  { get; set; }
    public bool    IsArchived         { get; set; }
    public string? Tags               { get; set; }
    public string? Description        { get; set; }
    public int?    EntityId           { get; set; }
    public string? EntityFullName     { get; set; }
}
