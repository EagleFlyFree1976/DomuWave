using DomuWave.Services.Dto.Dashboard;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Dashboard;

public class GetDashboardDeadlinesCommand : BaseCommand, IQuery<DashboardDeadlinesDto>
{
    public Guid TenantId { get; set; }
    public int  Days     { get; set; }

    public GetDashboardDeadlinesCommand() { }

    public GetDashboardDeadlinesCommand(int currentUserId, Guid tenantId, int days) : base(currentUserId)
    {
        TenantId = tenantId;
        Days     = days;
    }
}
