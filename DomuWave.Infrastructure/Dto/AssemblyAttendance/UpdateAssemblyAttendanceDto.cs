namespace DomuWave.Services.Dto.AssemblyAttendance;

public class UpdateAssemblyAttendanceDto
{
    public int?     AttendanceTypeId { get; set; }
    public string?  DelegateName     { get; set; }
    public decimal? MillesimalValue  { get; set; }
    public string?  Notes            { get; set; }
}
