using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class ApplyChartOfAccountsTemplateCommand : BaseCommand, IQuery<int>
{
    public int CondominiumId { get; set; }

    public ApplyChartOfAccountsTemplateCommand() { }

    public ApplyChartOfAccountsTemplateCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
