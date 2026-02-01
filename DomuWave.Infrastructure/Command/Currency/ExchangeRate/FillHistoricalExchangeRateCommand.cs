using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Currency.ExchangeRate;

public class FillHistoricalExchangeRateCommand : BaseCommand, IQuery<bool>
{
    public FillHistoricalExchangeRateCommand()
    {
    }

    public FillHistoricalExchangeRateCommand(DateTime targetDate)
    {
        TargetDate = targetDate;
    }

    public FillHistoricalExchangeRateCommand(int currentUserId, DateTime targetDate) : base(currentUserId)
    {
        TargetDate = targetDate;
    }

    public DateTime TargetDate { get; set; }
}