namespace DomuWave.Services.Dto.Assembly;

public class UpdateAssemblyDto
{
    public string?   Title          { get; set; }
    public int?      AssemblyTypeId { get; set; }
    public int?      FiscalYearId   { get; set; }
    public DateTime? ScheduledDate  { get; set; }
    public DateTime? ActualDate     { get; set; }
    public string?   Location       { get; set; }
    public string?   Notes          { get; set; }
    public string?   MinutesPath    { get; set; }
    public string?   BoardMembers   { get; set; }
    public string?   Minutes        { get; set; }
}
