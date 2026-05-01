using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class PlanAssemblyCommand : BaseCommand, IQuery<AssemblyReadDto>
{
    public int AssemblyId { get; set; }

    public PlanAssemblyCommand() { }
    public PlanAssemblyCommand(int currentUserId, int assemblyId) : base(currentUserId)
        => AssemblyId = assemblyId;
}
