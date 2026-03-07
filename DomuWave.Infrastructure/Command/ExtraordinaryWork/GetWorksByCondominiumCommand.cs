using DomuWave.Services.Dto.ExtraordinaryWork;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.ExtraordinaryWork;

public class GetWorksByCondominiumCommand : BaseCommand, IQuery<IList<ExtraordinaryWorkReadDto>>
{
    public int CondominiumId { get; }
    public GetWorksByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
