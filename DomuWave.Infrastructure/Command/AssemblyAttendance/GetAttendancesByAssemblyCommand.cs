using DomuWave.Services.Dto.AssemblyAttendance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAttendance;

public class GetAttendancesByAssemblyCommand : BaseCommand, IQuery<IList<AssemblyAttendanceReadDto>>
{
    public int AssemblyId { get; set; }

    public GetAttendancesByAssemblyCommand() { }
    public GetAttendancesByAssemblyCommand(int currentUserId, int assemblyId) : base(currentUserId)
        => AssemblyId = assemblyId;
}
