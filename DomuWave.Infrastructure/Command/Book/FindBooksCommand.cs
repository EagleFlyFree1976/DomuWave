using DomuWave.Services.Models.Dto;
using CPQ.Core.DTO;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
/// ritorna l'elenco di tutti i book che fanno capo ad un utente
/// </summary>
public class FindBooksCommand : BaseCommand,IQuery<IList<BookReadDto>>
{
    public FindBooksCommand()
    {
    }

    public FindBooksCommand(int currentUserId) : base(currentUserId)
    {
    }

    public int OwnerId { get; set; }
}