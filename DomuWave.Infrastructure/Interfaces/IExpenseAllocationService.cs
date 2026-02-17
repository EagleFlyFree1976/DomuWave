using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IExpenseAllocationService : IBaseService<ExpenseAllocation, long>
    {
        Task<IList<ExpenseAllocation>> GetByExpenseIdAsync(long expenseId);
        Task<IList<ExpenseAllocation>> GetByUnitIdAsync(int unitId);
        Task<decimal> GetTotalAllocationAsync(long expenseId);
        Task<bool> AllocateExpenseAsync(long expenseId, long userId);
    }
}
