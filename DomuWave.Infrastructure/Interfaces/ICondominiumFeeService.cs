using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumFeeService : IBaseService<CondominiumFee, long>
    {
            Task<IList<CondominiumFee>> GetByInstallmentIdAsync(int installmentId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<CondominiumFee>> GetByUnitIdAsync(int unitId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<CondominiumFee>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<CondominiumFee>> GetUnpaidFeesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<CondominiumFee>> GetOverdueFeesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalDueAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalBalanceAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> RecordPaymentAsync(long feeId, decimal amount, DateTime paymentDate, string paymentMethod, long userId, IUser currentUser, CancellationToken cancellationToken);

        // ─── AI Assistant (function calling) ──────────────────────────────────
        /// <summary>Quote di un condomino cercato per nome/cognome (via UnitOwner), opz. per condominio e anno.</summary>
        Task<IList<CondominiumFee>> GetFeesByOwnerNameAsync(Guid tenantId, string ownerName, int? condominiumId, int year, IUser currentUser, CancellationToken cancellationToken);

        /// <summary>Saldo residuo complessivo di un condomino cercato per nome/cognome.</summary>
        Task<decimal> GetTotalBalanceByOwnerAsync(Guid tenantId, string ownerName, int? condominiumId, IUser currentUser, CancellationToken cancellationToken);
    }
}
