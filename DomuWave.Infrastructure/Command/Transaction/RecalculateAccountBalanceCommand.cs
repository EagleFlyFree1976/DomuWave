using SimpleMediator.Events;

namespace DomuWave.Services.Command.Transaction;

public class RecalculateAccountBalanceCommand : BaseBookRelatedCommand, IEvent
{
    public long AccountId { get; set; }
}