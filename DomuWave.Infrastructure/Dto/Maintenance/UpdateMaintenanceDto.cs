using System.ComponentModel.DataAnnotations;

namespace DomuWave.Services.Dto.Maintenance;

public class UpdateMaintenanceDto
{
    public int?   SupplierId   { get; set; }

    [Required]
    public string Title        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category    { get; set; }
    public string  Priority    { get; set; } = "Medium";
    public string  Status      { get; set; } = "Open";

    [Required]
    public DateTime  ReportedDate  { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal?  EstimatedCost { get; set; }
    public decimal?  ActualCost    { get; set; }
    public string?   Notes         { get; set; }
    public string?   DocumentPath  { get; set; }
}
