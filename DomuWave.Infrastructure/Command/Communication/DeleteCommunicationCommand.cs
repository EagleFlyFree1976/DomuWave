using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class DeleteCommunicationCommand : BaseCommand, IQuery<bool>
{
    public int CommunicationId { get; set; }

    public DeleteCommunicationCommand() { }

    public DeleteCommunicationCommand(int currentUserId) : base(currentUserId) { }
    public DeleteCommunicationCommand(int currentUserId, int communicationId) : base(currentUserId)
    {
        CommunicationId = communicationId;
    }
}
