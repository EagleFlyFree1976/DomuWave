using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Maintenance;

public class MaintenanceReadDto : TraceEntityDTO<int>
{
    public int     CondominiumId   { get; set; }
    public string? CondominiumName { get; set; }
    public int?    SupplierId      { get; set; }
    public string? SupplierName    { get; set; }
    public string  Title           { get; set; } = string.Empty;
    public string? Description     { get; set; }
    public string? Category        { get; set; }
    public string  Priority        { get; set; } = string.Empty;
    public string  Status          { get; set; } = string.Empty;
    public DateTime  ReportedDate  { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal?  EstimatedCost { get; set; }
    public decimal?  ActualCost    { get; set; }
    public string?   Notes         { get; set; }
    public string?   DocumentPath  { get; set; }
}
