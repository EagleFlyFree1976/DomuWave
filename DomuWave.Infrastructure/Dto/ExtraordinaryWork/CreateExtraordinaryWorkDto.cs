using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.ExtraordinaryWork;

public class CreateExtraordinaryWorkDto
{
    [Required]
    public int    CondominiumId { get; set; }

    [Required]
    public string Title         { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public string? Category     { get; set; }
    public string  Status       { get; set; } = "Draft";
    public string  Priority     { get; set; } = "Medium";

    [Required]
    public DateTime  RequestedDate  { get; set; }
    public DateTime? ApprovedDate   { get; set; }
    public DateTime? StartDate      { get; set; }
    public DateTime? CompletedDate  { get; set; }
    public decimal?  ApprovedAmount { get; set; }
    public decimal?  ActualCost     { get; set; }
    public string?   Notes          { get; set; }
}
