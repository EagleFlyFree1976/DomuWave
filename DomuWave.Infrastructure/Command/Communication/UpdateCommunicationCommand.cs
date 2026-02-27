using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class UpdateCommunicationCommand : BaseCommand, IQuery<Models.Communication>
{
    public int CommunicationId { get; set; }
    public Models.Communication Entity { get; set; }

    public UpdateCommunicationCommand() { }

    public UpdateCommunicationCommand(int currentUserId) : base(currentUserId) { }
    public UpdateCommunicationCommand(int currentUserId, int communicationId, Models.Communication entity) : base(currentUserId)
    {
        CommunicationId = communicationId;
        Entity = entity;
    }
}
