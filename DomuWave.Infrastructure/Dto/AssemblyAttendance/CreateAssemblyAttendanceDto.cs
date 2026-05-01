namespace DomuWave.Services.Dto.AssemblyAttendance;

public class CreateAssemblyAttendanceDto
{
    public int      AssemblyId       { get; set; }
    public int      UnitOwnerId      { get; set; }
    public int      AttendanceTypeId { get; set; }
    public string?  DelegateName     { get; set; }
    public decimal  MillesimalValue  { get; set; }
    public string?  Notes            { get; set; }
}
