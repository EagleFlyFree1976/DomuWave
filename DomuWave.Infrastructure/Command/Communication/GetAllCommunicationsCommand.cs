using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetAllCommunicationsCommand : BaseCommand, IQuery<IList<Models.Communication>>
{

    public GetAllCommunicationsCommand() { }

    public GetAllCommunicationsCommand(int currentUserId) : base(currentUserId) { }
}
