using DomuWave.Services.Dto.UnitOpeningBalance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOpeningBalance;

public class SetUnitOpeningBalancesBulkCommand : BaseCommand, IQuery<bool>
{
    public SetUnitOpeningBalancesBulkDto Dto { get; set; }

    public SetUnitOpeningBalancesBulkCommand() { }
    public SetUnitOpeningBalancesBulkCommand(int currentUserId, SetUnitOpeningBalancesBulkDto dto) : base(currentUserId)
    {
        Dto = dto;
    }
}
