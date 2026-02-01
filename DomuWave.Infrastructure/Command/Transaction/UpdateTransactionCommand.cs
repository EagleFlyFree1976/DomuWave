using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
using DomuWave.Services.Models.Dto.Transaction;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;

/// <summary>
///     Crea una nuova transazione con i parametri impostati
/// </summary>
public class UpdateTransactionCommand : BaseBookRelatedCommand, IQuery<TransactionReadDto>
{
    private TransactionUpdateDto _updateDto;

    public UpdateTransactionCommand()
    {
    }

    public UpdateTransactionCommand(int currentUserId, long bookId) : base(currentUserId, bookId)
    {
    }

    public long TransactionId { get; set; }
    public TransactionUpdateDto updateDto
    {
        get => _updateDto;
        set
        {
            _updateDto = value;
            

        }
    }
}