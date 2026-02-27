using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class UpdateMillesimalTableCommand : BaseCommand, IQuery<Models.MillesimalTable>
{
    public int TableId { get; set; }
    public Models.MillesimalTable Entity { get; set; }

    public UpdateMillesimalTableCommand() { }

    public UpdateMillesimalTableCommand(int currentUserId) : base(currentUserId) { }
    public UpdateMillesimalTableCommand(int currentUserId, int tableId, Models.MillesimalTable entity) : base(currentUserId)
    {
        TableId = tableId;
        Entity = entity;
    }
}
