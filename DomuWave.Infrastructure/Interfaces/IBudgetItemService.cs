using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IBudgetItemService : IBaseService<BudgetItem, int>
    {
        Task<IList<BudgetItem>> GetByBudgetIdAsync(int budgetId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<BudgetItem>> GetByAccountIdAsync(int accountId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalAmountAsync(int budgetId, IUser currentUser, CancellationToken cancellationToken);
    }
}
