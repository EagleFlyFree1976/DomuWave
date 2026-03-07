using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.ExtraordinaryWork;

public class UpdateWorkQuoteDto
{
    public int?   SupplierId  { get; set; }
    public string? QuoteNumber { get; set; }

    [Required]
    public DateTime  QuoteDate  { get; set; }
    public DateTime? ValidUntil { get; set; }

    [Required]
    public decimal Amount { get; set; }
    public string  Status { get; set; } = "Pending";
    public string? Notes  { get; set; }
}
