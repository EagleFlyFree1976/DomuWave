namespace DomuWave.Services.Dto.MillesimalTable;

public class CreateMillesimalTableDto
{
    public int      CondominiumId   { get; set; }
    public string   Code            { get; set; } = string.Empty;
    public string?  Name            { get; set; }
    public string?  Description     { get; set; }
    public decimal  TotalMillesimal { get; set; }
}
