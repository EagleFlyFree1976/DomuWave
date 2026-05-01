using DomuWave.Services.Dto.Assembly;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Assembly;

public class CloseAssemblyCommand : BaseCommand, IQuery<AssemblyReadDto>
{
    public int      AssemblyId { get; set; }
    public DateTime ActualDate { get; set; }

    public CloseAssemblyCommand() { }
    public CloseAssemblyCommand(int currentUserId, int assemblyId, DateTime actualDate) : base(currentUserId)
    {
        AssemblyId = assemblyId;
        ActualDate = actualDate;
    }
}
