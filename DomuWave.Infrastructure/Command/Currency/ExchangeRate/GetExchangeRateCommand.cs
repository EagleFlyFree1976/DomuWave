using DomuWave.Services.Models.Dto.Currency;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Currency.ExchangeRate;

public class GetExchangeRateCommand : BaseCommand, IQuery<ExchangeRateHistoryReadDto>
{
    public GetExchangeRateCommand()
    {
        
    }
    public GetExchangeRateCommand(int currentUserId, DateTime targetDate, int fromCurrencyId, int toCurrencyId, bool exactlyMode) : base(currentUserId)
    {
        TargetDate = targetDate;
        FromCurrencyId = fromCurrencyId;
        ToCurrencyId = toCurrencyId;
        ExactlyMode = exactlyMode;
    }
    public DateTime TargetDate { get; set; }

    public int FromCurrencyId { get; set; }
    public int ToCurrencyId { get; set; }

    public bool ExactlyMode { get; set; }
}