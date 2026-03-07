using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.ExtraordinaryWork;

public class ExtraordinaryWorkReadDto : TraceEntityDTO<int>
{
    public int     CondominiumId   { get; set; }
    public string? CondominiumName { get; set; }
    public string  Title           { get; set; } = string.Empty;
    public string? Description     { get; set; }
    public string? Category        { get; set; }
    public string  Status          { get; set; } = string.Empty;
    public string  Priority        { get; set; } = string.Empty;
    public DateTime  RequestedDate  { get; set; }
    public DateTime? ApprovedDate   { get; set; }
    public DateTime? StartDate      { get; set; }
    public DateTime? CompletedDate  { get; set; }
    public decimal?  ApprovedAmount { get; set; }
    public decimal?  ActualCost     { get; set; }
    public string?   Notes          { get; set; }
    public int       QuoteCount     { get; set; }
    public int       ApprovedQuoteId { get; set; }
}
