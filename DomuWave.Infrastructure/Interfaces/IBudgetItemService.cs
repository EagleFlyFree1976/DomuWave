using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IBudgetItemService : IBaseService<BudgetItem, int>
    {
        Task<IList<BudgetItem>> GetByBudgetIdAsync(int budgetId);
        Task<IList<BudgetItem>> GetByAccountIdAsync(int accountId);
        Task<decimal> GetTotalAmountAsync(int budgetId);
    }
}
