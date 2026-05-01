namespace DomuWave.Services.Dto.AssemblyAgendaItem;

public class CreateAssemblyAgendaItemDto
{
    public int     AssemblyId  { get; set; }
    public int     OrderIndex  { get; set; }
    public string  Title       { get; set; } = string.Empty;
    public string? Description { get; set; }
}
