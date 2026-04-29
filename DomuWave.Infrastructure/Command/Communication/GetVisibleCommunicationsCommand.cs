using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetVisibleCommunicationsCommand : BaseCommand, IQuery<IList<CommunicationReadDto>>
{
    public int CondominiumId { get; set; }

    public GetVisibleCommunicationsCommand() { }
    public GetVisibleCommunicationsCommand(int currentUserId, int condominiumId) : base(currentUserId)
        => CondominiumId = condominiumId;
}
