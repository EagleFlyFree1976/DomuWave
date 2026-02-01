using DomuWave.Services.Models.Dto.Currency;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Currency.ExchangeRate;

public class CreateExchangeRateCommand : BaseCommand,IQuery<ExchangeRateHistoryReadDto>
{
    public CreateExchangeRateCommand()
    {
    }

    public CreateExchangeRateCommand(int currentUserId) : base(currentUserId)
    {
    }

    public ExchangeRateHistoryCreateUpdateDto Item { get; set; }
        
        
}