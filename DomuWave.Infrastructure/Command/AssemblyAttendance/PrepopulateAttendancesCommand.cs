using DomuWave.Services.Dto.AssemblyAttendance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAttendance;

public class PrepopulateAttendancesCommand : BaseCommand, IQuery<IList<AssemblyAttendanceReadDto>>
{
    public int AssemblyId { get; set; }

    public PrepopulateAttendancesCommand() { }
    public PrepopulateAttendancesCommand(int currentUserId, int assemblyId) : base(currentUserId)
        => AssemblyId = assemblyId;
}
