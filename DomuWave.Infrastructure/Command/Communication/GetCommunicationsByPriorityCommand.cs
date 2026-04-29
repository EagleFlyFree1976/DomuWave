using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetCommunicationsByPriorityCommand : BaseCommand, IQuery<IList<CommunicationReadDto>>
{
    public int    CondominiumId { get; set; }
    public string Priority      { get; set; }

    public GetCommunicationsByPriorityCommand() { }
    public GetCommunicationsByPriorityCommand(int currentUserId, int condominiumId, string priority) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        Priority      = priority;
    }
}
