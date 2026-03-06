using DomuWave.Services.Dto.MillesimalTable;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class GetMillesimalTableByIdCommand : BaseCommand, IQuery<MillesimalTableReadDto>
{
    public int TableId { get; set; }

    public GetMillesimalTableByIdCommand() { }

    public GetMillesimalTableByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetMillesimalTableByIdCommand(int currentUserId, int tableId) : base(currentUserId)
    {
        TableId = tableId;
    }
}
