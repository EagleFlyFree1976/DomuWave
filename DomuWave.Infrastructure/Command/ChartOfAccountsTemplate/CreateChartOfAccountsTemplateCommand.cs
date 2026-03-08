using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class CreateChartOfAccountsTemplateCommand : BaseCommand, IQuery<ChartOfAccountsTemplateReadDto>
{
    public Guid TenantId { get; set; }
    public CreateChartOfAccountsTemplateDto Dto { get; set; }
    public CreateChartOfAccountsTemplateCommand() { }
    public CreateChartOfAccountsTemplateCommand(int currentUserId, Guid tenantId, CreateChartOfAccountsTemplateDto dto)
        : base(currentUserId) { TenantId = tenantId; Dto = dto; }
}
