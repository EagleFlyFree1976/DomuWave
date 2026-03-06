using DomuWave.Services.Dto.ChartOfAccountsCategory;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategory;

public class GetChartOfAccountsCategoriesCommand : BaseCommand, IQuery<IList<ChartOfAccountsCategoryReadDto>>
{
    public Guid TenantId { get; set; }

    public GetChartOfAccountsCategoriesCommand() { }
    public GetChartOfAccountsCategoriesCommand(int currentUserId, Guid tenantId) : base(currentUserId)
        => TenantId = tenantId;
}
