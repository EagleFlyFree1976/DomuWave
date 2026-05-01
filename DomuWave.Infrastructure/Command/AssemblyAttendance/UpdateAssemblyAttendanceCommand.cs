using DomuWave.Services.Dto.AssemblyAttendance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAttendance;

public class UpdateAssemblyAttendanceCommand : BaseCommand, IQuery<AssemblyAttendanceReadDto>
{
    public int                         Id  { get; set; }
    public UpdateAssemblyAttendanceDto Dto { get; set; }

    public UpdateAssemblyAttendanceCommand() { }
    public UpdateAssemblyAttendanceCommand(int currentUserId, int id, UpdateAssemblyAttendanceDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
