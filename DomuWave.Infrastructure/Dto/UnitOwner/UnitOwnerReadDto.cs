using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.UnitOwner;

public class UnitOwnerReadDto : TraceEntityDTO<int>
{
    public int UnitId { get; set; }
    public long UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? OwnerType { get; set; }
    public decimal OwnershipQuota { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsResident { get; set; }
    public bool IsActive { get; set; }
    public bool IsAccessEnabled { get; set; }
    public string? Notes { get; set; }
}
