using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class GetMillesimalTableByIdCommand : BaseCommand, IQuery<Models.MillesimalTable>
{
    public int TableId { get; set; }

    public GetMillesimalTableByIdCommand() { }

    public GetMillesimalTableByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetMillesimalTableByIdCommand(int currentUserId, int tableId) : base(currentUserId)
    {
        TableId = tableId;
    }
}
