using DomuWave.Services.Models.Dto.Transaction;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;

public class UpdateMassiveTransactionCommand : BaseBookRelatedCommand, IQuery<bool>
{
    public UpdateMassiveTransactionCommand()
    {
    }
    public UpdateMassiveTransactionCommand(int currentUserId, long bookId) : base(currentUserId, bookId)
    {
    }
    public List<long> TransactionIds { get; set; } = new();
    public TransactionMassiveUpdateDto UpdateDto { get; set; } = new();
}