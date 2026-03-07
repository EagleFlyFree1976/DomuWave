using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class UpdateWorkCommand : BaseCommand, IQuery<ExtraordinaryWorkReadDto>
{
    public int                        WorkId { get; }
    public UpdateExtraordinaryWorkDto Dto    { get; }
    public UpdateWorkCommand(int currentUserId, int workId, UpdateExtraordinaryWorkDto dto) : base(currentUserId)
    { WorkId = workId; Dto = dto; }
}
