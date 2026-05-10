namespace DomuWave.Services.Dto.Staircase;

public class CreateStaircaseDto
{
    public int    CondominiumId { get; set; }
    public string Name         { get; set; }
    public bool   IsActive     { get; set; } = true;
}
