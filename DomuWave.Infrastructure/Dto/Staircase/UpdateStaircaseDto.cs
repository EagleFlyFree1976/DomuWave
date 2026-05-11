namespace DomuWave.Services.Dto.Staircase;

public class UpdateStaircaseDto
{
    public int?   BuildingId { get; set; }
    public string Name       { get; set; }
    public bool   IsActive   { get; set; }
}
