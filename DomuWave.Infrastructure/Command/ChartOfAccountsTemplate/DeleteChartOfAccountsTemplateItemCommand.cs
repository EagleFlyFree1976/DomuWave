using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class DeleteChartOfAccountsTemplateItemCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public DeleteChartOfAccountsTemplateItemCommand() { }
    public DeleteChartOfAccountsTemplateItemCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}
