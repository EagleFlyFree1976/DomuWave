using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class GetDefaultUserTenantCommand : BaseCommand, IQuery<Models.UserTenant>
{
    public long UserId { get; set; }

    public GetDefaultUserTenantCommand() { }

    public GetDefaultUserTenantCommand(int currentUserId) : base(currentUserId) { }
    public GetDefaultUserTenantCommand(int currentUserId, long userId) : base(currentUserId)
    {
        UserId = userId;
    }
}
