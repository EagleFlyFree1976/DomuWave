using DomuWave.Services.Dto.UnitOpeningBalance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOpeningBalance;

public class SetGroupOpeningBalanceCommand : BaseCommand, IQuery<bool>
{
    public int BillingGroupId { get; set; }
    public SetGroupOpeningBalanceDto Dto { get; set; }

    public SetGroupOpeningBalanceCommand() { }
    public SetGroupOpeningBalanceCommand(int currentUserId, int billingGroupId, SetGroupOpeningBalanceDto dto) : base(currentUserId)
    {
        BillingGroupId = billingGroupId;
        Dto = dto;
    }
}
