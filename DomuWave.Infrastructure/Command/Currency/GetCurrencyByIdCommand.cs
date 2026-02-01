using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
 
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class GetCurrencyByIdCommand : BaseCommand, IQuery<Models.Currency>
{
    public GetCurrencyByIdCommand()
    {
    }

    public GetCurrencyByIdCommand(int currentUserId) : base(currentUserId)
    {
    }

    public int Id { get; set; }
}