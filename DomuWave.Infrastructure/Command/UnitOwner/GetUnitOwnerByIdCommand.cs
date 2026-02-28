using DomuWave.Services.Dto.UnitOwner;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOwner;

public class GetUnitOwnerByIdCommand : BaseCommand, IQuery<UnitOwnerReadDto>
{
    public int Id { get; set; }

    public GetUnitOwnerByIdCommand() { }

    public GetUnitOwnerByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetUnitOwnerByIdCommand(int currentUserId, int id) : base(currentUserId)
    {
        Id = id;
    }
}
