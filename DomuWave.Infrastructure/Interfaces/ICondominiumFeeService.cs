using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumFeeService : IBaseService<CondominiumFee, long>
    {
        Task<IList<CondominiumFee>> GetByInstallmentIdAsync(int installmentId);
        Task<IList<CondominiumFee>> GetByUnitIdAsync(int unitId);
        Task<IList<CondominiumFee>> GetByUserIdAsync(long userId);
        Task<IList<CondominiumFee>> GetUnpaidFeesAsync(int condominiumId);
        Task<IList<CondominiumFee>> GetOverdueFeesAsync(int condominiumId);
        Task<decimal> GetTotalDueAsync(long userId);
        Task<decimal> GetTotalBalanceAsync(long userId);
        Task<bool> RecordPaymentAsync(long feeId, decimal amount, DateTime paymentDate, string paymentMethod, long userId);
    }
}
