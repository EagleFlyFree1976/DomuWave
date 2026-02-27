using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class SetDefaultUserTenantCommand : BaseCommand, IQuery<Models.UserTenant>
{
    public long UserId { get; set; }
    public int UserTenantId { get; set; }

    public SetDefaultUserTenantCommand() { }

    public SetDefaultUserTenantCommand(int currentUserId) : base(currentUserId) { }
    public SetDefaultUserTenantCommand(int currentUserId, long userId, int userTenantId) : base(currentUserId)
    {
        UserId = userId;
        UserTenantId = userTenantId;
    }
}
