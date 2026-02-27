using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class GetUnpaidFeesCommand : BaseCommand, IQuery<IList<Models.CondominiumFee>>
{
    public int CondominiumId { get; set; }

    public GetUnpaidFeesCommand() { }

    public GetUnpaidFeesCommand(int currentUserId) : base(currentUserId) { }
    public GetUnpaidFeesCommand(int currentUserId, int condominiumId) : base(currentUserId)
    {
        CondominiumId = condominiumId;
    }
}
