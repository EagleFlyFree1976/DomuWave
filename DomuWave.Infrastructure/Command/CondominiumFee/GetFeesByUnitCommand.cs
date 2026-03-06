using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesByUnitCommand : BaseCommand, IQuery<IList<CondominiumFeeReadDto>>
{
    public int UnitId { get; set; }

    public GetFeesByUnitCommand() { }

    public GetFeesByUnitCommand(int currentUserId) : base(currentUserId) { }
    public GetFeesByUnitCommand(int currentUserId, int unitId) : base(currentUserId)
    {
        UnitId = unitId;
    }
}
