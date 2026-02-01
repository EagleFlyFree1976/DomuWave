using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
/// ritorna l'elenco di tutti gli account che fanno riferimento ad un dato book
/// </summary>
public class FindAccountsCommand : BaseBookRelatedCommand, IQuery<IList<AccountReadDto>>
{
    public FindAccountsCommand()
    {
    }

    public FindAccountsCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public long? BookId { get; set; }
    public int OwnerId { get; set; }
}