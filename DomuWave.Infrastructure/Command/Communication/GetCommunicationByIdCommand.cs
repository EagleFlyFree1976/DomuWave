using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetCommunicationByIdCommand : BaseCommand, IQuery<CommunicationReadDto?>
{
    public int CommunicationId { get; set; }

    public GetCommunicationByIdCommand() { }
    public GetCommunicationByIdCommand(int currentUserId, int communicationId) : base(currentUserId)
        => CommunicationId = communicationId;
}
