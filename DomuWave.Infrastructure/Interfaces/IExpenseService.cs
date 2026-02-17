using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IExpenseService : IBaseService<Expense, long>
    {
        Task<IList<Expense>> GetByCondominiumIdAsync(int condominiumId);
        Task<IList<Expense>> GetByDateRangeAsync(int condominiumId, DateTime startDate, DateTime endDate);
        Task<IList<Expense>> GetBySupplierIdAsync(int supplierId);
        Task<IList<Expense>> GetByTypeAsync(int condominiumId, string expenseType);
        Task<IList<Expense>> GetUnpaidExpensesAsync(int condominiumId);
        Task<decimal> GetTotalExpensesAsync(int condominiumId, DateTime startDate, DateTime endDate);
        Task<bool> MarkAsPaidAsync(long expenseId, DateTime paymentDate, string paymentMethod, long userId);
    }
}
