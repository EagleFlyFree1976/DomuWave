using DomuWave.Services.Helper;
using DomuWave.Services.Models;
using DomuWave.Services.Models.Dto.PaymentMethod;
using DomuWave.Services.Models.Dto.Transaction;
using CPQ.Core.Memberships;

namespace DomuWave.Services.Interfaces;

public interface ITransactionService
{
    IQueryable<Transaction> GetQueryable();

    Task<Transaction> GetById(long itemId, long currentUserBookId, IUser currentUser, CancellationToken cancellationToken);

    Task<Transaction> Create(TransactionCreateDto item, long currentUserBookId, bool fromImport, IUser currentUser,
        CancellationToken cancellationToken);
    Task<Transaction> Update(long entityId, TransactionUpdateDto updateDto, IUser currentUser, CancellationToken cancellationToken);
    
 
     


    Task Delete(long transactionId, long bookId, IUser currentUser, CancellationToken cancellationToken);

    Task<PagedResult<Transaction>> Find(FindTransaction filters, int? pageNumber, int? pageSize, string sortBy, bool asc, IUser currentUser, CancellationToken cancellationToken);

    Task RecalculateAccountBalance(long accountId, CancellationToken cancellationToken);
    Task<decimal> CalculateAccountBalance(long transactionId, long? prevAccountId, long actualAccountId, DateTime? prevTransactionDate, DateTime currentTransactionDate, long bookId, IUser currentUser, CancellationToken cancellationToken);
}