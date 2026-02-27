using SimpleMediator.Queries;

namespace DomuWave.Services.Command.CondominiumFee;

public class DeleteCondominiumFeeCommand : BaseCommand, IQuery<bool>
{
    public long FeeId { get; set; }

    public DeleteCondominiumFeeCommand() { }

    public DeleteCondominiumFeeCommand(int currentUserId) : base(currentUserId) { }
    public DeleteCondominiumFeeCommand(int currentUserId, long feeId) : base(currentUserId)
    {
        FeeId = feeId;
    }
}
