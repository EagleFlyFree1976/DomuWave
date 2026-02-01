using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface ITransactionStatusService : IService
{

    Task<TransactionStatus> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<TransactionStatus> GetByIdAsync(int statusId, CancellationToken cancellationToken);
    Task<IList<TransactionStatus>> FindAll(CancellationToken cancellationToken);
}