using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Fault;

public class FaultReadDto : TraceEntityDTO<int>
{
    public int     CondominiumId   { get; set; }
    public string  CondominiumName { get; set; } = string.Empty;
    public int?    UnitId          { get; set; }
    public string? UnitDisplayName { get; set; }
    public int     StatusId        { get; set; }
    public string  StatusName      { get; set; } = string.Empty;
    public long    ReporterUserId  { get; set; }
    public string  ReporterFullName { get; set; } = string.Empty;
    public string  Title           { get; set; } = string.Empty;
    public string  Description     { get; set; } = string.Empty;
    public int     MessageCount    { get; set; }
}
