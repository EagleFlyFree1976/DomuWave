using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class GetChartOfAccountsTemplateItemsCommand : BaseCommand, IQuery<IList<ChartOfAccountsTemplateItemReadDto>>
{
    public int TemplateId { get; set; }
    public GetChartOfAccountsTemplateItemsCommand() { }
    public GetChartOfAccountsTemplateItemsCommand(int currentUserId, int templateId) : base(currentUserId)
        => TemplateId = templateId;
}
