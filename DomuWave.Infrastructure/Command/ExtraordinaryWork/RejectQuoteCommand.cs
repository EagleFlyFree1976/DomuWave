using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class RejectQuoteCommand : BaseCommand, IQuery<WorkQuoteReadDto>
{
    public int QuoteId { get; }
    public RejectQuoteCommand(int currentUserId, int quoteId) : base(currentUserId)
        => QuoteId = quoteId;
}
