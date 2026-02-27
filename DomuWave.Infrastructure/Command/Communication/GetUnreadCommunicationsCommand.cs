using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetUnreadCommunicationsCommand : BaseCommand, IQuery<IList<Models.Communication>>
{
    public int CondominiumId { get; set; }
    public long UserId { get; set; }

    public GetUnreadCommunicationsCommand() { }

    public GetUnreadCommunicationsCommand(int currentUserId) : base(currentUserId) { }
    public GetUnreadCommunicationsCommand(int currentUserId, int condominiumId, long userId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        UserId = userId;
    }
}
