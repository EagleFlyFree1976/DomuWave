using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAttendance;

public class DeleteAssemblyAttendanceCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteAssemblyAttendanceCommand() { }
    public DeleteAssemblyAttendanceCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
