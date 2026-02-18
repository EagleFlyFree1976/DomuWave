using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Domain.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IExpenseAllocationService : IBaseService<ExpenseAllocation, long>
    {
        Task<IList<ExpenseAllocation>> GetByExpenseIdAsync(long expenseId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<ExpenseAllocation>> GetByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalAllocationAsync(long expenseId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> AllocateExpenseAsync(long expenseId, long userId, IUser currentUser, CancellationToken cancellationToken);
    }
}
