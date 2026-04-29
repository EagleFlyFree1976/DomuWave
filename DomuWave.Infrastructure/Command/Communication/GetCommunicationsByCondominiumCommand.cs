using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetCommunicationsByCondominiumCommand : BaseCommand, IQuery<IList<CommunicationReadDto>>
{
    public int  CondominiumId   { get; set; }
    public bool IncludeArchived { get; set; }

    public GetCommunicationsByCondominiumCommand() { }
    public GetCommunicationsByCondominiumCommand(int currentUserId, int condominiumId, bool includeArchived = false) : base(currentUserId)
    {
        CondominiumId   = condominiumId;
        IncludeArchived = includeArchived;
    }
}
