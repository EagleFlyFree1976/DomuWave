using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IReceiptService : IBaseService<Receipt, long>
    {
        Task<IList<Receipt>> GetByCondominiumIdAsync(int condominiumId);
        Task<IList<Receipt>> GetByUserIdAsync(long userId);
        Task<IList<Receipt>> GetByUnitIdAsync(int unitId);
        Task<IList<Receipt>> GetByDateRangeAsync(int condominiumId, DateTime startDate, DateTime endDate);
        Task<IList<Receipt>> GetUnreconciledReceiptsAsync(int condominiumId);
        Task<bool> ReconcileReceiptAsync(long receiptId, long feeId, long userId);
        Task<decimal> GetTotalReceiptsAsync(int condominiumId, DateTime startDate, DateTime endDate);
    }
}
