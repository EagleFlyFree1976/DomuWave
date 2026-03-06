namespace DomuWave.Services.Dto.UnitMillesimal;

public class CreateUnitMillesimalDto
{
    public int     MillesimalTableId { get; set; }
    public int     UnitId            { get; set; }
    public decimal Millesimal        { get; set; }
    public string? Notes             { get; set; }
}
