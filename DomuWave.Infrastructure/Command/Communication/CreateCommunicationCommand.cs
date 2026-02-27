using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class CreateCommunicationCommand : BaseCommand, IQuery<Models.Communication>
{
    public Models.Communication Entity { get; set; }

    public CreateCommunicationCommand() { }

    public CreateCommunicationCommand(int currentUserId) : base(currentUserId) { }
    public CreateCommunicationCommand(int currentUserId, Models.Communication entity) : base(currentUserId)
    {
        Entity = entity;
    }
}
