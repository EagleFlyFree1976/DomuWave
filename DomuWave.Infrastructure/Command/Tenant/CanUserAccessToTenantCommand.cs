using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Tenant;

public class CanUserAccessToTenantCommand : BaseCommand, IQuery<bool>
{
    /// <summary>ID del tenant da recuperare.</summary>
    public Guid TenantId { get; set; }

    public CanUserAccessToTenantCommand()
    {
    }

    public CanUserAccessToTenantCommand(int currentUserId, Guid tenantId) : base(currentUserId)
    {
        TenantId = tenantId;
    }
}