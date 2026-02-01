using DomuWave.Services.Models.Dto.Currency;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Currency.ExchangeRate;

public class GetExchangeRateByIdCommand : BaseCommand, IQuery<ExchangeRateHistoryReadDto>
{
    public GetExchangeRateByIdCommand()
    {
    }
    public GetExchangeRateByIdCommand(int currentUserId) : base(currentUserId)
    {
    }
    public long ExchangeRateId { get; set; }
}