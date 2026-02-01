using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class CanAccessToBookCommand : BaseCommand, IQuery<bool>
{
    public long BookId { get; set; }
    public CanAccessToBookCommand(int currentUserId, long bookId) : base(currentUserId)
    {
        BookId = bookId;
    }
}