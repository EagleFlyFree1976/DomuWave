using DomuWave.Services.Models.Dto.Currency;
using Bogus.DataSets;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Currency.ExchangeRate;

public class UpdateExchangeRateCommand : BaseCommand,IQuery<ExchangeRateHistoryReadDto>
{
    public UpdateExchangeRateCommand()
    {
    }

    public UpdateExchangeRateCommand(int currentUserId) : base(currentUserId)
    {
    }

    public long ExchangeRateHistoryId { get; set; }
    public ExchangeRateHistoryCreateUpdateDto Item { get; set; }
        
        
}