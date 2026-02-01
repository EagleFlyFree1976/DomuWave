using DomuWave.Services.Models.Dto.Currency;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

/// <summary>
/// Aggiorna una currency esistente
/// </summary>
public class UpdateCurrencyCommand : BaseCommand,IQuery<CurrencyReadDto>
{
    public UpdateCurrencyCommand()
    {
    }

    public UpdateCurrencyCommand(int currentUserId) : base(currentUserId)
    {
    }

    public int CurrencyId { get; set; }
    public CurrencyCreateUpdateDto Item { get; set; }
        
        
}