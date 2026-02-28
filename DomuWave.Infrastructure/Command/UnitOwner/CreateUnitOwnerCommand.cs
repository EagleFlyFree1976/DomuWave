using DomuWave.Services.Dto.UnitOwner;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOwner;

public class CreateUnitOwnerCommand : BaseCommand, IQuery<UnitOwnerReadDto>
{
    public CreateUnitOwnerDto Dto { get; set; }

    public CreateUnitOwnerCommand() { }

    public CreateUnitOwnerCommand(int currentUserId) : base(currentUserId) { }
    public CreateUnitOwnerCommand(int currentUserId, CreateUnitOwnerDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
