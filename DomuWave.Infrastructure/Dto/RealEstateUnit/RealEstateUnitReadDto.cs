using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.RealEstateUnit;

public class RealEstateUnitReadDto : TraceEntityDTO<int>
{
    public Guid TenantId { get; set; }
    public int CondominiumId { get; set; }
    public string CondominiumName { get; set; }
    public string Staircase { get; set; }
    public int Floor { get; set; }
    public string InternalNumber { get; set; }
    public string Subordinate { get; set; }
    public string Category { get; set; }
    public decimal? CadastralIncome { get; set; }
    public decimal? AreaSqm { get; set; }
    public decimal? Rooms { get; set; }
    public string UnitType { get; set; }
    public string OccupancyStatus { get; set; }
    public string Notes { get; set; }
    public string? DisplayName { get; set; }
    public int    NumeroAbitanti { get; set; }
    public bool IsActive { get; set; }
}
