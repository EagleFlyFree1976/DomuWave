using DomuWave.Services.Dto.AssemblyAgendaItem;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAgendaItem;

public class GetAgendaItemsByAssemblyCommand : BaseCommand, IQuery<IList<AssemblyAgendaItemReadDto>>
{
    public int AssemblyId { get; set; }

    public GetAgendaItemsByAssemblyCommand() { }
    public GetAgendaItemsByAssemblyCommand(int currentUserId, int assemblyId) : base(currentUserId)
        => AssemblyId = assemblyId;
}
