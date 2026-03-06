namespace DomuWave.Services.Dto.MillesimalTable;

public class CreateMillesimalTableDto
{
    public int      CondominiumId   { get; set; }
    public string   Code            { get; set; } = string.Empty;
    public decimal  TotalMillesimal { get; set; }
    public bool     IsActive        { get; set; } = true;
}
