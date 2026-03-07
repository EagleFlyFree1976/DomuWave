using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class ApproveQuoteCommand : BaseCommand, IQuery<WorkQuoteReadDto>
{
    public int QuoteId { get; }
    public ApproveQuoteCommand(int currentUserId, int quoteId) : base(currentUserId)
        => QuoteId = quoteId;
}
