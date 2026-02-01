using DomuWave.Services.Models;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

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