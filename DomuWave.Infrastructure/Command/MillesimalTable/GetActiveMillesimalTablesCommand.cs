using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class GetActiveMillesimalTablesCommand : BaseCommand, IQuery<IList<Models.MillesimalTable>>
{
    public int CondominiumId { get; set; }

    public GetActiveMillesimalTablesCommand() { }

    public GetActiveMillesimalTablesCommand(int currentUserId) : base(currentUserId) { }
    public GetActiveMillesimalTablesCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
