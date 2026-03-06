using DomuWave.Services.Dto.MillesimalTable;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class GetActiveMillesimalTablesCommand : BaseCommand, IQuery<IList<MillesimalTableReadDto>>
{
    public int CondominiumId { get; set; }

    public GetActiveMillesimalTablesCommand() { }

    public GetActiveMillesimalTablesCommand(int currentUserId) : base(currentUserId) { }
    public GetActiveMillesimalTablesCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
