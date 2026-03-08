namespace DomuWave.Services.Dto.RealEstateUnit;

public class UpdateRealEstateUnitDto
{
    public string? Staircase { get; set; }
    public int Floor { get; set; }
    public string? InternalNumber { get; set; }
    public string? Subordinate { get; set; }
    public string? Category { get; set; }
    public decimal? CadastralIncome { get; set; }
    public decimal? AreaSqm { get; set; }
    public decimal? Rooms { get; set; }
    public string? UnitType { get; set; }
    public string? OccupancyStatus { get; set; }
    public string? Notes { get; set; }
    public string? DisplayName { get; set; }
    public int     NumeroAbitanti { get; set; } = 1;
    public bool IsActive { get; set; }
}
