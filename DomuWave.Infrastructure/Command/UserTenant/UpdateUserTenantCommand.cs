using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class UpdateUserTenantCommand : BaseCommand, IQuery<Models.UserTenant>
{
    public int UserTenantId { get; set; }
    public Models.UserTenant Entity { get; set; }

    public UpdateUserTenantCommand() { }

    public UpdateUserTenantCommand(int currentUserId) : base(currentUserId) { }
    public UpdateUserTenantCommand(int currentUserId, int userTenantId, Models.UserTenant entity) : base(currentUserId)
    {
        UserTenantId = userTenantId;
        Entity = entity;
    }
}
