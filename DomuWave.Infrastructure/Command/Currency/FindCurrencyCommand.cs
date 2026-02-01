using DomuWave.Services.Models.Dto.Currency;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;
/// <summary>

/// ritorna un elenco di currency in base ai criteri di ricerca
/// </summary>
public class FindCurrencyCommand : BaseCommand, IQuery<IList<CurrencyReadDto>>
{
    public FindCurrencyCommand()
    {
    }

    public FindCurrencyCommand(int currentUserId) : base(currentUserId)
    {
    }

    public string Q { get; set; }
}