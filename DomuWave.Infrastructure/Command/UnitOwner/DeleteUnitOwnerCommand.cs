using DomuWave.Services.Dto.UnitOwner;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOwner;

public class DeleteUnitOwnerCommand : BaseCommand, IQuery<UnitOwnerReadDto>
{
    public int Id { get; set; }

    public DeleteUnitOwnerCommand() { }

    public DeleteUnitOwnerCommand(int currentUserId) : base(currentUserId) { }
    public DeleteUnitOwnerCommand(int currentUserId, int id) : base(currentUserId)
    {
        Id = id;
    }
}
