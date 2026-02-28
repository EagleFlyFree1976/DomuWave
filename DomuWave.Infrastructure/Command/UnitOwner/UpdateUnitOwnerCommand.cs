using DomuWave.Services.Dto.UnitOwner;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOwner;

public class UpdateUnitOwnerCommand : BaseCommand, IQuery<UnitOwnerReadDto>
{
    public int Id { get; set; }
    public UpdateUnitOwnerDto Dto { get; set; }

    public UpdateUnitOwnerCommand() { }

    public UpdateUnitOwnerCommand(int currentUserId) : base(currentUserId) { }
    public UpdateUnitOwnerCommand(int currentUserId, int id, UpdateUnitOwnerDto dto) : base(currentUserId)
    {
        Id  = id;
        Dto = dto;
    }
}
