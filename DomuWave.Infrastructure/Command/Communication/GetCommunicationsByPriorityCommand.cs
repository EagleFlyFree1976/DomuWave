using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetCommunicationsByPriorityCommand : BaseCommand, IQuery<IList<Models.Communication>>
{
    public int CondominiumId { get; set; }
    public string Priority { get; set; }

    public GetCommunicationsByPriorityCommand() { }

    public GetCommunicationsByPriorityCommand(int currentUserId) : base(currentUserId) { }
    public GetCommunicationsByPriorityCommand(int currentUserId, int condominiumId, string priority) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Priority = priority;
    }
}
