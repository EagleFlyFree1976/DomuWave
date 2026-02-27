using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetCommunicationByIdCommand : BaseCommand, IQuery<Models.Communication>
{
    public int CommunicationId { get; set; }

    public GetCommunicationByIdCommand() { }

    public GetCommunicationByIdCommand(int currentUserId) : base(currentUserId) { }
    public GetCommunicationByIdCommand(int currentUserId, int communicationId) : base(currentUserId)
    {
        CommunicationId = communicationId;
    }
}
