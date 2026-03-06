using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class CopyChartOfAccountsCommand : BaseCommand, IQuery<int>
{
    public int SourceCondominiumId { get; set; }
    public int TargetCondominiumId { get; set; }

    public CopyChartOfAccountsCommand() { }
    public CopyChartOfAccountsCommand(int currentUserId, int sourceCondominiumId, int targetCondominiumId)
        : base(currentUserId)
    {
        SourceCondominiumId = sourceCondominiumId;
        TargetCondominiumId = targetCondominiumId;
    }
}
