using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesTotalBalanceCommand : BaseCommand, IQuery<decimal>
{
    public long UserId { get; set; }

    public GetFeesTotalBalanceCommand() { }

    public GetFeesTotalBalanceCommand(int currentUserId) : base(currentUserId) { }
    public GetFeesTotalBalanceCommand(int currentUserId, long userId) : base(currentUserId)
    {
        UserId = userId;
    }
}
