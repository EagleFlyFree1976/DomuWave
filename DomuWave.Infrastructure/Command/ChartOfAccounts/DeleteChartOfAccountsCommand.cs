using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccounts;

public class DeleteChartOfAccountsCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }

    public DeleteChartOfAccountsCommand() { }
    public DeleteChartOfAccountsCommand(int currentUserId, int id) : base(currentUserId)
        => Id = id;
}
