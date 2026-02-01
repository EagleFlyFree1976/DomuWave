using DomuWave.Services.Models.Dto;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class GetBookByIdCommand : BaseBookRelatedCommand, IQuery<BookReadDto>
{
    public GetBookByIdCommand()
    {
    }

    public GetBookByIdCommand(int currentUserId, long currentBookId, long bookId) : base(currentUserId, currentBookId)
    {
        BookId = bookId;
    }

    public long BookId { get; set; }
}