using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Transaction;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;

/// <summary>
///     Crea una nuova transazione con i parametri impostati
/// </summary>
public class CreateTransactionCommand : BaseBookRelatedCommand, IQuery<TransactionReadDto>
{
    private TransactionCreateDto _createDto;

    public CreateTransactionCommand()
    {
    }

    public CreateTransactionCommand(int currentUserId, long bookId) : base(currentUserId, bookId)
    {
    }

    public TransactionCreateDto CreateDto
    {
        get => _createDto;
        set
        {
            _createDto = value;
            

        }
    }
}