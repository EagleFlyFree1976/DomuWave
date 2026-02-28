using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class GetUserTenantsByTenantCommand : BaseCommand, IQuery<IList<Models.UserTenant>>
{
    public Guid TenantId { get; set; }

    public GetUserTenantsByTenantCommand() { }

    public GetUserTenantsByTenantCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}
