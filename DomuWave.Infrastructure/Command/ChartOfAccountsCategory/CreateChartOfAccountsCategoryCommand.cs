using DomuWave.Services.Dto.ChartOfAccountsCategory;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ChartOfAccountsCategory;

public class CreateChartOfAccountsCategoryCommand : BaseCommand, IQuery<ChartOfAccountsCategoryReadDto>
{
    public Guid                            TenantId { get; set; }
    public CreateChartOfAccountsCategoryDto Dto     { get; set; }

    public CreateChartOfAccountsCategoryCommand() { }
    public CreateChartOfAccountsCategoryCommand(int currentUserId, Guid tenantId, CreateChartOfAccountsCategoryDto dto)
        : base(currentUserId)
    {
        TenantId = tenantId;
        Dto      = dto;
    }
}
