using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class GetUserTenantsByUserCommand : BaseCommand, IQuery<IList<Models.UserTenant>>
{
    public long UserId { get; set; }

    public GetUserTenantsByUserCommand() { }

    public GetUserTenantsByUserCommand(int currentUserId) : base(currentUserId) { }
    public GetUserTenantsByUserCommand(int currentUserId, long userId) : base(currentUserId)
    {
        UserId = userId;
    }
}
