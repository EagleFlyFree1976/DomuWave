using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class GetQuotesByWorkCommand : BaseCommand, IQuery<IList<WorkQuoteReadDto>>
{
    public int WorkId { get; }
    public GetQuotesByWorkCommand(int currentUserId, int workId) : base(currentUserId)
        => WorkId = workId;
}
