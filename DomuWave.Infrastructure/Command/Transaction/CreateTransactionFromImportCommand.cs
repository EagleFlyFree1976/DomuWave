using DomuWave.Services.Models.Dto.Transaction;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;

public class CreateTransactionFromImportCommand : BaseBookRelatedCommand, IQuery<TransactionReadDto>
{
    private TransactionCreateDto _createDto;

    public CreateTransactionFromImportCommand()
    {
    }

    public CreateTransactionFromImportCommand(int currentUserId, long bookId) : base(currentUserId, bookId)
    {
    }

    public long ImportTransactionId { get; set; }
    public TransactionCreateDto CreateDto
    {
        get => _createDto;
        set
        {
            _createDto = value;


        }
    }
}