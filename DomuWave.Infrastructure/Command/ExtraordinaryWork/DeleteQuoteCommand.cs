using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class DeleteQuoteCommand : BaseCommand, IQuery<bool>
{
    public int QuoteId { get; }
    public DeleteQuoteCommand(int currentUserId, int quoteId) : base(currentUserId)
        => QuoteId = quoteId;
}
