using DomuWave.Services.Dto.Staircase;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Staircase;

public class GetStaircasesByCondominiumCommand : BaseCommand, IQuery<IList<StaircaseReadDto>>
{
    public int CondominiumId { get; set; }

    public GetStaircasesByCondominiumCommand() { }
    public GetStaircasesByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
