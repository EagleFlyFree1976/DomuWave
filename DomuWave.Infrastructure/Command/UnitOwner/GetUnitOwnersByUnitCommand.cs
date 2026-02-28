using DomuWave.Services.Dto.UnitOwner;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOwner;

public class GetUnitOwnersByUnitCommand : BaseCommand, IQuery<IList<UnitOwnerReadDto>>
{
    public int UnitId { get; set; }

    public GetUnitOwnersByUnitCommand() { }

    public GetUnitOwnersByUnitCommand(int currentUserId) : base(currentUserId) { }
    public GetUnitOwnersByUnitCommand(int currentUserId, int unitId) : base(currentUserId)
    {
        UnitId = unitId;
    }
}
