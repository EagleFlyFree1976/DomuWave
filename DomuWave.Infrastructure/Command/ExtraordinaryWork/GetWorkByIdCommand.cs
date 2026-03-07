using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class GetWorkByIdCommand : BaseCommand, IQuery<ExtraordinaryWorkReadDto>
{
    public int WorkId { get; }
    public GetWorkByIdCommand(int currentUserId, int workId) : base(currentUserId)
        => WorkId = workId;
}
