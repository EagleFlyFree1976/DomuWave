namespace DomuWave.Services.Dto.Assembly;

public class CreateAssemblyDto
{
    public int       CondominiumId  { get; set; }
    public int?      FiscalYearId   { get; set; }
    public string    Title          { get; set; } = string.Empty;
    public int       AssemblyTypeId { get; set; }
    public DateTime  ScheduledDate  { get; set; }
    public string?   Location       { get; set; }
    public string?   Notes          { get; set; }
}
