using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.Currency;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Book;

public class ConvertToCurrencyCommand : BaseCommand, IQuery<ConvertResult>
{
    public Models.Currency SourceCurrency { get; set; }
    public Models.Currency DestinationCurrency { get; set; }

    public decimal Amount { get; set; }
    public DateTime TargetDate { get; set; }


    public ConvertToCurrencyCommand()
    {
    }

    public ConvertToCurrencyCommand(int currentUserId, Models.Currency sourceCurrency, Models.Currency destinationCurrency, decimal amount, DateTime targetDate) : base(currentUserId)
    {
        SourceCurrency = sourceCurrency;
        DestinationCurrency = destinationCurrency;
        Amount = amount;
        TargetDate = targetDate;
    }
}