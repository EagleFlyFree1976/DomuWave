using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetFeesTotalDueCommand : BaseCommand, IQuery<decimal>
{
    public long UserId { get; set; }

    public GetFeesTotalDueCommand() { }

    public GetFeesTotalDueCommand(int currentUserId) : base(currentUserId) { }
    public GetFeesTotalDueCommand(int currentUserId, long userId) : base(currentUserId)
    {
        UserId = userId;
    }
}
