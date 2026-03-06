using DomuWave.Services.Dto.MillesimalTable;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class GetMillesimalTablesByCondominiumCommand : BaseCommand, IQuery<IList<MillesimalTableReadDto>>
{
    public int CondominiumId { get; set; }

    public GetMillesimalTablesByCondominiumCommand() { }

    public GetMillesimalTablesByCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public GetMillesimalTablesByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
