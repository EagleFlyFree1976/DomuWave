using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.ExtraordinaryWork;

public class WorkQuoteDocumentReadDto : TraceEntityDTO<int>
{
    public int     QuoteId      { get; set; }
    public string  Title        { get; set; } = string.Empty;
    public string  FileName     { get; set; } = string.Empty;
    public string  FilePath     { get; set; } = string.Empty;
    public long    FileSize     { get; set; }
    public string  MimeType     { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string? Notes        { get; set; }
}
