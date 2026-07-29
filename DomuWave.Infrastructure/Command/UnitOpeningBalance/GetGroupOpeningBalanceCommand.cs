using DomuWave.Services.Dto.UnitOpeningBalance;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOpeningBalance;

public class GetGroupOpeningBalanceCommand : BaseCommand, IQuery<UnitOpeningBalanceReadDto>
{
    public int BillingGroupId { get; set; }
    public int FiscalYearId   { get; set; }

    public GetGroupOpeningBalanceCommand() { }
    public GetGroupOpeningBalanceCommand(int currentUserId, int billingGroupId, int fiscalYearId) : base(currentUserId)
    {
        BillingGroupId = billingGroupId;
        FiscalYearId   = fiscalYearId;
    }
}
