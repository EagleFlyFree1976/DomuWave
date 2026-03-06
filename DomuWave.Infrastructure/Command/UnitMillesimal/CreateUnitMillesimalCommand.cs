using DomuWave.Services.Dto.UnitMillesimal;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitMillesimal;

public class CreateUnitMillesimalCommand : BaseCommand, IQuery<UnitMillesimalReadDto>
{
    public CreateUnitMillesimalDto Dto { get; set; }

    public CreateUnitMillesimalCommand() { }
    public CreateUnitMillesimalCommand(int currentUserId, CreateUnitMillesimalDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
