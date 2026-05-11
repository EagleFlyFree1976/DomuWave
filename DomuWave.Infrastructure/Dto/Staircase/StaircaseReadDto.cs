using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Staircase;

public class StaircaseReadDto : TraceEntityDTO<int>
{
    public int     CondominiumId   { get; set; }
    public string  CondominiumName { get; set; }
    public int?    BuildingId      { get; set; }
    public string? BuildingName    { get; set; }
    public string  Name            { get; set; }
    public bool    IsActive        { get; set; }
}
