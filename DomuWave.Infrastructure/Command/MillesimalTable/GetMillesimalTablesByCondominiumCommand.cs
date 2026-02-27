using SimpleMediator.Queries;

namespace DomuWave.Services.Command.MillesimalTable;

public class GetMillesimalTablesByCondominiumCommand : BaseCommand, IQuery<IList<Models.MillesimalTable>>
{
    public int CondominiumId { get; set; }

    public GetMillesimalTablesByCondominiumCommand() { }

    public GetMillesimalTablesByCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public GetMillesimalTablesByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
