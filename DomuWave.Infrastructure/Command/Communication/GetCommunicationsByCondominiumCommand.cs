using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetCommunicationsByCondominiumCommand : BaseCommand, IQuery<IList<Models.Communication>>
{
    public int CondominiumId { get; set; }

    public GetCommunicationsByCondominiumCommand() { }

    public GetCommunicationsByCondominiumCommand(int currentUserId) : base(currentUserId) { }
    public GetCommunicationsByCondominiumCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
