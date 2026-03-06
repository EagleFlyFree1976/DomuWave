using DomuWave.Services.Dto.UnitMillesimal;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitMillesimal;

public class GetUnitMillesimalsByTableCommand : BaseCommand, IQuery<IList<UnitMillesimalReadDto>>
{
    public int MillesimalTableId { get; set; }

    public GetUnitMillesimalsByTableCommand() { }
    public GetUnitMillesimalsByTableCommand(int currentUserId, int millesimalTableId) : base(currentUserId)
    {
        MillesimalTableId = millesimalTableId;
    }
}
