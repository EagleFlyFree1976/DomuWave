using DomuWave.Services.Models;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Menu;

public class PopulateAccountsEventCommand :    PopulateMenuEventCommand, IQuery<IList<MenuItemDto>>
{
    public PopulateAccountsEventCommand()
    {
    }

    public PopulateAccountsEventCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }
}