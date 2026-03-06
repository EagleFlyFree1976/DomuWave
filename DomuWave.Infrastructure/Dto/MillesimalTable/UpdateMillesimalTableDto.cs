namespace DomuWave.Services.Dto.MillesimalTable;

public class UpdateMillesimalTableDto
{
    public string   Code            { get; set; } = string.Empty;
    public decimal  TotalMillesimal { get; set; }
    public bool     IsActive        { get; set; }
}
