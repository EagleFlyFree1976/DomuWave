using DomuWave.Services.Dto.AssemblyAttendance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAttendance;

public class CreateAssemblyAttendanceCommand : BaseCommand, IQuery<AssemblyAttendanceReadDto>
{
    public CreateAssemblyAttendanceDto Dto { get; set; }

    public CreateAssemblyAttendanceCommand() { }
    public CreateAssemblyAttendanceCommand(int currentUserId, CreateAssemblyAttendanceDto dto) : base(currentUserId)
        => Dto = dto;
}
