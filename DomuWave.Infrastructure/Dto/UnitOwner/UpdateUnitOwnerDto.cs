namespace DomuWave.Services.Dto.UnitOwner;

public class UpdateUnitOwnerDto
{
    public string? OwnerType { get; set; }
    public decimal OwnershipQuota { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsResident { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
