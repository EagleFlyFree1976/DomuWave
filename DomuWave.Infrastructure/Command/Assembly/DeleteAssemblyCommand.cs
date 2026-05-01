using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class DeleteAssemblyCommand : BaseCommand, IQuery<bool>
{
    public int AssemblyId { get; set; }

    public DeleteAssemblyCommand() { }
    public DeleteAssemblyCommand(int currentUserId, int assemblyId) : base(currentUserId)
        => AssemblyId = assemblyId;
}
