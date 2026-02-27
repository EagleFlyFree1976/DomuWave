using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetCommunicationsByTypeCommand : BaseCommand, IQuery<IList<Models.Communication>>
{
    public int CondominiumId { get; set; }
    public string CommunicationType { get; set; }

    public GetCommunicationsByTypeCommand() { }

    public GetCommunicationsByTypeCommand(int currentUserId) : base(currentUserId) { }
    public GetCommunicationsByTypeCommand(int currentUserId, int condominiumId, string communicationType) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        CommunicationType = communicationType;
    }
}
