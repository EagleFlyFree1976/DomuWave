using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class OpenAssemblyCommand : BaseCommand, IQuery<AssemblyReadDto>
{
    public int AssemblyId { get; set; }

    public OpenAssemblyCommand() { }
    public OpenAssemblyCommand(int currentUserId, int assemblyId) : base(currentUserId)
        => AssemblyId = assemblyId;
}
