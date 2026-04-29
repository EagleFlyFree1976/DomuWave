using DomuWave.Services.Dto.Communication;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Communication;

public class GetUnreadCommunicationsCommand : BaseCommand, IQuery<IList<CommunicationReadDto>>
{
    public int  CondominiumId { get; set; }
    public long UserId        { get; set; }

    public GetUnreadCommunicationsCommand() { }
    public GetUnreadCommunicationsCommand(int currentUserId, int condominiumId, long userId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
        UserId        = userId;
    }
}
