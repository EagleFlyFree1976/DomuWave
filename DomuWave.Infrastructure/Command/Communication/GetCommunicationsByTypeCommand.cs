using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetCommunicationsByTypeCommand : BaseCommand, IQuery<IList<CommunicationReadDto>>
{
    public int    CondominiumId     { get; set; }
    public string CommunicationType { get; set; }

    public GetCommunicationsByTypeCommand() { }
    public GetCommunicationsByTypeCommand(int currentUserId, int condominiumId, string communicationType) : base(currentUserId)
    {
        CondominiumId     = condominiumId;
        CommunicationType = communicationType;
    }
}
