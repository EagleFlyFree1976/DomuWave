namespace DomuWave.Services.Dto.RealEstateUnit;

public class CreateRealEstateUnitDto
{
    public int  CondominiumId { get; set; }
    public int? BuildingId   { get; set; }
    public int? StaircaseId  { get; set; }
    public int Floor { get; set; }
    public string? InternalNumber { get; set; }
    public string? Sheet { get; set; }
    public string? Parcel { get; set; }
    public string? Subordinate { get; set; }
    public string? Category { get; set; }
    public decimal? CadastralIncome { get; set; }
    public decimal? AreaSqm { get; set; }
    public decimal? Rooms { get; set; }
    public string? UnitType { get; set; }
    public string? OccupancyStatus { get; set; }
    public string? Notes { get; set; }
    public int     NumeroAbitanti { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
