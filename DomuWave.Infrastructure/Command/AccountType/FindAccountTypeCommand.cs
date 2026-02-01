using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;
/// <summary>

/// ritorna un elenco di accounttype in base ai criteri di ricerca
/// </summary>
public class FindAccountTypeCommand : BaseCommand, IQuery<IList<AccountTypeReadDto>>
{
    public FindAccountTypeCommand()
    {
    }

    public FindAccountTypeCommand(int currentUserId) : base(currentUserId)
    {
    }

    public string Q { get; set; }
}