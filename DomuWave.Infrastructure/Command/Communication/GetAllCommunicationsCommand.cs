using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetAllCommunicationsCommand : BaseCommand, IQuery<IList<CommunicationReadDto>>
{
    public GetAllCommunicationsCommand() { }
    public GetAllCommunicationsCommand(int currentUserId) : base(currentUserId) { }
}
