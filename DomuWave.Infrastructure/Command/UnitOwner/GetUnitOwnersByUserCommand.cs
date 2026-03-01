using DomuWave.Services.Dto.UnitOwner;
using SimpleMediator.Core;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.UnitOwner;

public class GetUnitOwnersByUserCommand : BaseCommand, IQuery<IList<UserUnitOwnerDto>>
{
    public long UserId { get; set; }

    public GetUnitOwnersByUserCommand() { }

    public GetUnitOwnersByUserCommand(int currentUserId, long userId) : base(currentUserId)
    {
        UserId = userId;
    }
}
