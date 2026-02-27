using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class PublishCommunicationCommand : BaseCommand, IQuery<bool>
{
    public int CommunicationId { get; set; }

    public PublishCommunicationCommand() { }

    public PublishCommunicationCommand(int currentUserId) : base(currentUserId) { }
    public PublishCommunicationCommand(int currentUserId, int communicationId) : base(currentUserId)
    {
        CommunicationId = communicationId;
    }
}
