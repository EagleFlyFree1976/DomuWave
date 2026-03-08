using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class SaveChartOfAccountsTemplateItemCommand : BaseCommand, IQuery<ChartOfAccountsTemplateItemReadDto>
{
    public int? Id  { get; set; }   // null = create
    public SaveChartOfAccountsTemplateItemDto Dto { get; set; }
    public SaveChartOfAccountsTemplateItemCommand() { }
    public SaveChartOfAccountsTemplateItemCommand(int currentUserId, int? id, SaveChartOfAccountsTemplateItemDto dto)
        : base(currentUserId) { Id = id; Dto = dto; }
}
