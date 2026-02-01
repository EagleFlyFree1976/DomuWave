using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Currency.ExchangeRate;

public class DeleteExchangeRateByIdCommand : BaseCommand, IQuery<bool>
{
    public DeleteExchangeRateByIdCommand()
    {
    }

    public DeleteExchangeRateByIdCommand(int currentUserId) : base(currentUserId)
    {
    }

    public long ExchangeRateId { get; set; }
}