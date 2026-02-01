using DomuWave.Services.Models;

namespace DomuWave.Services.Helper;

public class FindTransaction
{
    public long? TargetAccountId { get; set; }
    public long? AccountId { get; set; }
    public long BookId { get; set; }

    public DateRange Date { get; set; }

    public TransactionType? TransactionType { get; set; }

    public FlowDirection? FlowDirection { get; set; }
    public int? CategoryId { get; set; }

    public string Note { get; set; }


    public int? Status { get; set; }
}