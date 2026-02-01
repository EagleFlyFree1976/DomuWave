using DomuWave.Services.Models;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class GetDefaultCurrencyCommand : BaseCommand, IQuery<Models.Currency>
{
    public GetDefaultCurrencyCommand()
    {
    }

    public GetDefaultCurrencyCommand(int currentUserId) : base(currentUserId)
    {
    }

 
}