using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;
using DomuWave.Services.Dto.Expense;

namespace DomuWave.Services.Interfaces
{
    public interface IExpenseService : IBaseService<Expense, long>
    {
        // ─── AI Assistant (function calling) ──────────────────────────────────
        /// <summary>Riepilogo aggregato delle spese di un condominio per un anno fiscale.</summary>
        Task<ExpenseSummaryDto> GetExpenseSummaryAsync(Guid tenantId, int condominiumId, int year, IUser currentUser, CancellationToken cancellationToken);

        Task<Expense> GetByIdAsync(long id, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetByCondominiumIdAsync(int condominiumId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetByDateRangeAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetBySupplierIdAsync(int supplierId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetByTypeAsync(int condominiumId, int expenseTypeId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetUnpaidExpensesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalExpensesAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> MarkAsPaidAsync(long expenseId, DateTime paymentDate, int? paymentMethodId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> MarkAsUnpaidAsync(long expenseId, IUser currentUser, CancellationToken cancellationToken);
    }
}
