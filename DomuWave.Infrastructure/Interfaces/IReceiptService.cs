using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Domain.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IReceiptService : IBaseService<Receipt, long>
    {
        Task<IList<Receipt>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Receipt>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Receipt>> GetByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Receipt>> GetByDateRangeAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Receipt>> GetUnreconciledReceiptsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> ReconcileReceiptAsync(long receiptId, long feeId, long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalReceiptsAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser, CancellationToken cancellationToken);
    }
}
