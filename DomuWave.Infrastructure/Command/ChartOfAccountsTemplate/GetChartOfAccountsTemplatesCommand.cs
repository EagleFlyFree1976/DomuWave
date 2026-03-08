using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsTemplate;

public class GetChartOfAccountsTemplatesCommand : BaseCommand, IQuery<IList<ChartOfAccountsTemplateReadDto>>
{
    public Guid TenantId { get; set; }
    public GetChartOfAccountsTemplatesCommand() { }
    public GetChartOfAccountsTemplatesCommand(int currentUserId, Guid tenantId) : base(currentUserId)
        => TenantId = tenantId;
}
