using NHibernate;
using SimpleMediator.Commands;
using SimpleMediator.Queries;

namespace DomuWave.Services.Command.Transaction;


/// <summary>
/// Calcola il saldo dell'account associato alla transazione specificata
/// </summary>
public class CalculateTransactionAccountBalanceCommand : BaseBookRelatedCommand, IQuery<decimal>
{
    public DateTime? PrevTransactionDate { get; set; }
    public DateTime CurrentTransactionDate { get; set; }
    public long ActualAccountId { get; set; }
    public long? PrevAccountId { get; set; }
    public long TransactionId { get; set; }
}