using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class UpdateChartOfAccountsTemplateCommand : BaseCommand, IQuery<ChartOfAccountsTemplateReadDto>
{
    public int Id { get; set; }
    public UpdateChartOfAccountsTemplateDto Dto { get; set; }
    public UpdateChartOfAccountsTemplateCommand() { }
    public UpdateChartOfAccountsTemplateCommand(int currentUserId, int id, UpdateChartOfAccountsTemplateDto dto)
        : base(currentUserId) { Id = id; Dto = dto; }
}
