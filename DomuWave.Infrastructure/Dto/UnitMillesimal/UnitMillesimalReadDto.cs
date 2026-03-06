using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.UnitMillesimal;

public class UnitMillesimalReadDto : TraceEntityDTO<int>
{
    public int     MillesimalTableId { get; set; }
    public int     UnitId            { get; set; }
    public string  UnitNumber        { get; set; } = string.Empty;
    public int     UnitFloor         { get; set; }
    public string? UnitStaircase     { get; set; }
    public decimal Millesimal        { get; set; }
    public string? Notes             { get; set; }
}
