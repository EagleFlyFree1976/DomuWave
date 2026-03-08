using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class DeleteChartOfAccountsTemplateCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteChartOfAccountsTemplateCommand() { }
    public DeleteChartOfAccountsTemplateCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}
