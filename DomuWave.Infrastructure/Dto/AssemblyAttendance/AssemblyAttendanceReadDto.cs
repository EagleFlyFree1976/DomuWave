using CPQ.Core.DTO;

namespace DomuWave.Services.Dto.AssemblyAttendance;

public class AssemblyAttendanceReadDto : TraceEntityDTO<int>
{
    public int      AssemblyId          { get; set; }
    public int      UnitOwnerId         { get; set; }
    public string?  OwnerFirstName      { get; set; }
    public string?  OwnerLastName       { get; set; }
    public string?  UnitInternalNumber  { get; set; }
    public int      AttendanceTypeId    { get; set; }
    public string?  AttendanceTypeName  { get; set; }
    public string?  DelegateName        { get; set; }
    public decimal  MillesimalValue     { get; set; }
    public string?  Notes               { get; set; }
}
