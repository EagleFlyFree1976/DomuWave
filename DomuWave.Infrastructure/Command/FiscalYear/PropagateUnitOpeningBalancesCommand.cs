using SimpleMediator.Queries;

namespace DomuWave.Services.Command.FiscalYear;

public class PropagateUnitOpeningBalancesCommand : BaseCommand, IQuery<int>
{
    public int FiscalYearId { get; set; }

    public PropagateUnitOpeningBalancesCommand() { }

    public PropagateUnitOpeningBalancesCommand(int currentUserId, int fiscalYearId) : base(currentUserId)
    {
        FiscalYearId = fiscalYearId;
    }
}
