using DomuWave.Services.Dto.MillesimalTable;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class UpdateMillesimalTableCommand : BaseCommand, IQuery<MillesimalTableReadDto>
{
    public int                      TableId { get; set; }
    public UpdateMillesimalTableDto Dto     { get; set; }

    public UpdateMillesimalTableCommand() { }
    public UpdateMillesimalTableCommand(int currentUserId, int tableId, UpdateMillesimalTableDto dto) : base(currentUserId)
    {
        TableId = tableId;
        Dto     = dto;
    }
}
