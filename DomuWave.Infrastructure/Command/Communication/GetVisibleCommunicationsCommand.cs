using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetVisibleCommunicationsCommand : BaseCommand, IQuery<IList<Models.Communication>>
{
    public int CondominiumId { get; set; }

    public GetVisibleCommunicationsCommand() { }

    public GetVisibleCommunicationsCommand(int currentUserId) : base(currentUserId) { }
    public GetVisibleCommunicationsCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
