using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command;

public class GetMenuItemsCommand : BaseBookRelatedCommand, IQuery<IList<MenuItemDto>>
{
    public GetMenuItemsCommand()
    {
    }

    public GetMenuItemsCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public long? BookId { get; set; }
    public int OwnerId { get; set; }
}