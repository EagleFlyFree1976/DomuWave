using DomuWave.Services.Models.Dto.Transaction;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;

public class GetTransactionByIdCommand : BaseBookRelatedCommand, IQuery<TransactionReadDto>
{
    public long Id { get; set; }

    public GetTransactionByIdCommand(long id)
    {
        Id = id;
    }

    public GetTransactionByIdCommand(long id,int currentUserId, long bookId ) : base(currentUserId, bookId)
    {
        Id = id;
    }

    public GetTransactionByIdCommand()
    {
    }
}