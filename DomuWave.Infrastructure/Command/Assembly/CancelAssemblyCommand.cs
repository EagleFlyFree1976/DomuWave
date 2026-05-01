using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class CancelAssemblyCommand : BaseCommand, IQuery<AssemblyReadDto>
{
    public int AssemblyId { get; set; }

    public CancelAssemblyCommand() { }
    public CancelAssemblyCommand(int currentUserId, int assemblyId) : base(currentUserId)
        => AssemblyId = assemblyId;
}
