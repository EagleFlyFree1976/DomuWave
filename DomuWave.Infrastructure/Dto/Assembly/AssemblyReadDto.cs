using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.Assembly;

public class AssemblyReadDto : TraceEntityDTO<int>
{
    public int       CondominiumId    { get; set; }
    public string?   CondominiumName  { get; set; }
    public int?      FiscalYearId     { get; set; }
    public string?   FiscalYearName   { get; set; }
    public string    Title            { get; set; } = string.Empty;
    public int       AssemblyTypeId   { get; set; }
    public string?   AssemblyTypeName { get; set; }
    public int       StatusId         { get; set; }
    public string?   StatusName       { get; set; }
    public DateTime  ScheduledDate    { get; set; }
    public DateTime? ActualDate       { get; set; }
    public string?   Location         { get; set; }
    public string?   Notes            { get; set; }
    public string?   MinutesPath      { get; set; }
    public string?   BoardMembers     { get; set; }
    public string?   Minutes          { get; set; }
    public int       AgendaItemCount  { get; set; }
    public int       AttendanceCount  { get; set; }
    public int?      CommunicationId    { get; set; }
    public string?   CommunicationTitle { get; set; }
}
