using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.ExtraordinaryWork;

public class WorkQuoteReadDto : TraceEntityDTO<int>
{
    public int     WorkId        { get; set; }
    public string? WorkTitle     { get; set; }
    public int?    SupplierId    { get; set; }
    public string? SupplierName  { get; set; }
    public string? QuoteNumber   { get; set; }
    public DateTime  QuoteDate   { get; set; }
    public DateTime? ValidUntil  { get; set; }
    public decimal   Amount      { get; set; }
    public string    Status      { get; set; } = string.Empty;
    public bool      IsApproved  { get; set; }
    public string?   Notes       { get; set; }
    public IList<WorkQuoteDocumentReadDto> Documents { get; set; } = new List<WorkQuoteDocumentReadDto>();
}
