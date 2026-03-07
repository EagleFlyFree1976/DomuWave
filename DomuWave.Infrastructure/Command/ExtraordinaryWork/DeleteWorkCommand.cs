using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class DeleteWorkCommand : BaseCommand, IQuery<bool>
{
    public int WorkId { get; }
    public DeleteWorkCommand(int currentUserId, int workId) : base(currentUserId)
        => WorkId = workId;
}
