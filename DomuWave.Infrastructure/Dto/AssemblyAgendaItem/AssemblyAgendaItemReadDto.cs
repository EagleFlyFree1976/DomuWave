using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.AssemblyAgendaItem;

public class AssemblyAgendaItemReadDto : TraceEntityDTO<int>
{
    public int     AssemblyId      { get; set; }
    public int     OrderIndex      { get; set; }
    public string  Title           { get; set; } = string.Empty;
    public string? Description     { get; set; }
    public string? Resolution      { get; set; }
    public int     VoteResultId    { get; set; }
    public string? VoteResultName  { get; set; }
}
