namespace DomuWave.Services.Dto.AssemblyAgendaItem;

public class UpdateAssemblyAgendaItemDto
{
    public int?    OrderIndex    { get; set; }
    public string? Title        { get; set; }
    public string? Description  { get; set; }
    public string? Resolution   { get; set; }
    public int?    VoteResultId { get; set; }
}
