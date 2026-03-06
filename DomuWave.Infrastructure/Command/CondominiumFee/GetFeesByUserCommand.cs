using DomuWave.Services.Dto.CondominiumFee;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesByUserCommand : BaseCommand, IQuery<IList<CondominiumFeeReadDto>>
{
    public long UserId { get; set; }

    public GetFeesByUserCommand() { }

    public GetFeesByUserCommand(int currentUserId) : base(currentUserId) { }
    public GetFeesByUserCommand(int currentUserId, long userId) : base(currentUserId)
    {
        UserId = userId;
    }
}
