using DomuWave.Services.Dto.Dashboard;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Dashboard;

public class GetDashboardSummaryCommand : BaseCommand, IQuery<DashboardSummaryDto>
{
    public Guid TenantId { get; set; }

    public GetDashboardSummaryCommand() { }

    public GetDashboardSummaryCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
