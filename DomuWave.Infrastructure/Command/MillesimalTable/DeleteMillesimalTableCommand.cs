using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class DeleteMillesimalTableCommand : BaseCommand, IQuery<bool>
{
    public int TableId { get; set; }

    public DeleteMillesimalTableCommand() { }

    public DeleteMillesimalTableCommand(int currentUserId) : base(currentUserId) { }
    public DeleteMillesimalTableCommand(int currentUserId, int tableId) : base(currentUserId)
    {
        TableId = tableId;
    }
}
