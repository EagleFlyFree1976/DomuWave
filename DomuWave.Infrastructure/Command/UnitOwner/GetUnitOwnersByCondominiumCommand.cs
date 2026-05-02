using DomuWave.Services.Dto.UnitOwner;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOwner;

public class GetUnitOwnersByCondominiumCommand : BaseCommand, IQuery<IList<UnitOwnerReadDto>>
{
    public int CondominiumId { get; set; }

    public GetUnitOwnersByCondominiumCommand() { }
    public GetUnitOwnersByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
