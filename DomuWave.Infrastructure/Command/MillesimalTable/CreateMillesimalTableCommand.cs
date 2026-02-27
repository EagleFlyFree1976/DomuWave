using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class CreateMillesimalTableCommand : BaseCommand, IQuery<Models.MillesimalTable>
{
    public Models.MillesimalTable Entity { get; set; }

    public CreateMillesimalTableCommand() { }

    public CreateMillesimalTableCommand(int currentUserId) : base(currentUserId) { }
    public CreateMillesimalTableCommand(int currentUserId, Models.MillesimalTable entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
