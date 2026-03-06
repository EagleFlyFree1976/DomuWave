using DomuWave.Services.Dto.MillesimalTable;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class CreateMillesimalTableCommand : BaseCommand, IQuery<MillesimalTableReadDto>
{
    public CreateMillesimalTableDto Dto { get; set; }

    public CreateMillesimalTableCommand() { }
    public CreateMillesimalTableCommand(int currentUserId, CreateMillesimalTableDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
