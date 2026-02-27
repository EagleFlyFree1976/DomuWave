using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class GetUserTenantByIdCommand : BaseCommand, IQuery<Models.UserTenant>
{
    public int UserTenantId { get; set; }

    public GetUserTenantByIdCommand() { }

    public GetUserTenantByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetUserTenantByIdCommand(int currentUserId, int userTenantId) : base(currentUserId)
    {
        UserTenantId = userTenantId;
    }
}
