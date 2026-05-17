namespace DomuWave.Services.Dto.Fault;

public class CreateFaultDto
{
    public int     CondominiumId { get; set; }
    public int?    UnitId        { get; set; }
    public string  Title         { get; set; } = string.Empty;
    public string  Description   { get; set; } = string.Empty;
}
