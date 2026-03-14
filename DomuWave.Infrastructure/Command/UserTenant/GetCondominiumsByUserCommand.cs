using DomuWave.Services.Dto.Condominium;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UserTenant;

public class GetCondominiumsByUserCommand : BaseCommand, IQuery<IList<UserCondominiumDto>>
{
    public long UserId { get; set; }

    public GetCondominiumsByUserCommand() { }

    public GetCondominiumsByUserCommand(int currentUserId, long userId) : base(currentUserId)
    {
        UserId = userId;
    }
}
