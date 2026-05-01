using SimpleMediator.Queries;

namespace DomuWave.Services.Command.AssemblyAgendaItem;

public class DeleteAssemblyAgendaItemCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteAssemblyAgendaItemCommand() { }
    public DeleteAssemblyAgendaItemCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
