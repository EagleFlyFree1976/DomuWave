using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.MillesimalTable;

public class MillesimalTableReadDto : TraceEntityDTO<int>
{
    public int      CondominiumId   { get; set; }
    public string   Code            { get; set; } = string.Empty;
    public string?  Name            { get; set; }
    public string?  Description     { get; set; }
    public decimal  TotalMillesimal { get; set; }
    public bool     IsActive        { get; set; }
}
