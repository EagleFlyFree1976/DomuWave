using DomuWave.Services.Models.Dto.Transaction;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;

public class DeleteTransactionByIdCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public long Id { get; set; }

    public DeleteTransactionByIdCommand(long id)
    {
        Id = id;
    }

    public DeleteTransactionByIdCommand(long id,int currentUserId, long bookId ) : base(currentUserId, bookId)
    {
        Id = id;
    }

    public DeleteTransactionByIdCommand()
    {
    }
}