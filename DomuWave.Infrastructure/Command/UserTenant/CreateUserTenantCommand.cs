using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class CreateUserTenantCommand : BaseCommand, IQuery<Models.UserTenant>
{
    public Models.UserTenant Entity { get; set; }

    public CreateUserTenantCommand() { }

    public CreateUserTenantCommand(int currentUserId) : base(currentUserId) { }
    public CreateUserTenantCommand(int currentUserId, Models.UserTenant entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
