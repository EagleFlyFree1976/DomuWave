using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class DeleteAccountByIdCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public DeleteAccountByIdCommand()
    {
    }

    public DeleteAccountByIdCommand(int currentUserId, long currentBookId) : base(currentUserId, currentBookId)
    {
    }

    public long BookId { get; set; }
    public long AccountId { get; set; }
}