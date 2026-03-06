using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IExpenseService : IBaseService<Expense, long>
    {
        Task<IList<Expense>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetByDateRangeAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetBySupplierIdAsync(int supplierId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetByTypeAsync(int condominiumId, int expenseTypeId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Expense>> GetUnpaidExpensesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalExpensesAsync(int condominiumId, DateTime startDate, DateTime endDate, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> MarkAsPaidAsync(long expenseId, DateTime paymentDate, string paymentMethod, IUser currentUser, CancellationToken cancellationToken);
    }
}
