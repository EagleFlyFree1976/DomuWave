using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class SetDefaultChartOfAccountsTemplateCommand : BaseCommand, IQuery<bool>
{
    public int Id { get; set; }
    public SetDefaultChartOfAccountsTemplateCommand() { }
    public SetDefaultChartOfAccountsTemplateCommand(int currentUserId, int id) : base(currentUserId) => Id = id;
}
