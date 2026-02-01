using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
///  Ritorna il dettaglio dell'account selezionato
/// </summary>
public class GetAccountByIdCommand : BaseBookRelatedCommand, IQuery<AccountReadDto>
{
    public GetAccountByIdCommand()
    {
    }

    public GetAccountByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public long BookId { get; set; }
    public long AccountId { get; set; }
    public int OwnerId { get; set; }
}