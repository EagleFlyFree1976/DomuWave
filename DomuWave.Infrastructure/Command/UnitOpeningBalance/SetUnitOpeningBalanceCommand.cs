using DomuWave.Services.Dto.UnitOpeningBalance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOpeningBalance;

public class SetUnitOpeningBalanceCommand : BaseCommand, IQuery<UnitOpeningBalanceReadDto>
{
    public int                      UnitId { get; set; }
    public SetUnitOpeningBalanceDto Dto    { get; set; }

    public SetUnitOpeningBalanceCommand() { }
    public SetUnitOpeningBalanceCommand(int currentUserId, int unitId, SetUnitOpeningBalanceDto dto) : base(currentUserId)
    {
        UnitId = unitId;
        Dto    = dto;
    }
}
