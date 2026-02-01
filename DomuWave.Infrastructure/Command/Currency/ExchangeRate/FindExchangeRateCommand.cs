using DomuWave.Services.Helper;
using DomuWave.Services.Models.Dto.Currency;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Currency.ExchangeRate;

public class FindExchangeRateCommand : BasePagedCommand, IQuery<PagedResult<ExchangeRateHistoryReadDto>>
{
    public FindExchangeRateCommand()
    {
    }
    public FindExchangeRateCommand(int currentUserId) : base(currentUserId)
    {
    }
    public DateTime? TargetDate { get; set; }

    public int? ToCurrencyId { get; set; }

    public bool ExactlyMode { get; set; }
}