using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class DeleteUserTenantCommand : BaseCommand, IQuery<bool>
{
    public int UserTenantId { get; set; }

    public DeleteUserTenantCommand() { }

    public DeleteUserTenantCommand(int currentUserId) : base(currentUserId) { }
    public DeleteUserTenantCommand(int currentUserId, int userTenantId) : base(currentUserId)
    {
        UserTenantId = userTenantId;
    }
}
