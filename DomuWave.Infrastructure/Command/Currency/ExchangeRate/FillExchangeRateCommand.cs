using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Currency.ExchangeRate;

public class FillExchangeRateCommand : BaseCommand, IQuery<bool>
{
    public FillExchangeRateCommand()
    {
    }

    public FillExchangeRateCommand(int currentUserId) : base(currentUserId)
    {
    }
}