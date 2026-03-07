using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class GetWorksByStatusCommand : BaseCommand, IQuery<IList<ExtraordinaryWorkReadDto>>
{
    public int    CondominiumId { get; }
    public string Status        { get; }
    public GetWorksByStatusCommand(int currentUserId, int condominiumId, string status) : base(currentUserId)
    { CondominiumId = condominiumId; Status = status; }
}
