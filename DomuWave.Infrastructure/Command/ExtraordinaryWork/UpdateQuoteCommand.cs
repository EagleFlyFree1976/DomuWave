using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class UpdateQuoteCommand : BaseCommand, IQuery<WorkQuoteReadDto>
{
    public int               QuoteId { get; }
    public UpdateWorkQuoteDto Dto    { get; }
    public UpdateQuoteCommand(int currentUserId, int quoteId, UpdateWorkQuoteDto dto) : base(currentUserId)
    { QuoteId = quoteId; Dto = dto; }
}
