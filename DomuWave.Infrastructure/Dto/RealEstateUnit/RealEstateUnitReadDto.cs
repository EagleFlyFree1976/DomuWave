using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.RealEstateUnit;

public class RealEstateUnitReadDto : TraceEntityDTO<int>
{
    public Guid TenantId { get; set; }
    public int CondominiumId { get; set; }
    public string CondominiumName { get; set; }
    public int?   BuildingId   { get; set; }
    public string BuildingName { get; set; }
    public int?   StaircaseId   { get; set; }
    public string StaircaseName { get; set; }
    public int Floor { get; set; }
    public string InternalNumber { get; set; }
    public string? Sheet { get; set; }
    public string? Parcel { get; set; }
    public string Subordinate { get; set; }
    public string Category { get; set; }
    public decimal? CadastralIncome { get; set; }
    public decimal? AreaSqm { get; set; }
    public decimal? Rooms { get; set; }
    public string UnitType { get; set; }
    public string OccupancyStatus { get; set; }
    public string Notes { get; set; }
    public string? DisplayName { get; set; }
    public bool IsDisplayNameOverridden { get; set; }
    public int    NumeroAbitanti { get; set; }
    public bool IsActive { get; set; }
    public int OwnersCount  { get; set; }
    public int TenantsCount { get; set; }
}
