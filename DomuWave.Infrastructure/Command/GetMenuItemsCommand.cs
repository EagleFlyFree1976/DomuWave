using DomuWave.Services.Models;

using SimpleMediator.Queries;

namespace DomuWave.Services.Command;

public class GetMenuItemsCommand : BaseTenantRelatedCommand, IQuery<IList<MenuItemDto>>
{
    public GetMenuItemsCommand()
    {
    }

    public GetMenuItemsCommand(int currentUserId, Guid currentTenantId) : base(currentUserId, currentTenantId)
    {
    }

    public Guid? TenantId { get; set; }
    public int OwnerId { get; set; }
}