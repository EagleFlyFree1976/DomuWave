using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IBudgetService : IBaseService<Budget, int>
    {
        Task<Budget> GetByIdAsync(int id, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Budget>> GetByCondominiumIdAsync(int condominiumId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<Budget> GetByYearAndTypeAsync(int condominiumId, int year, BudgetType type, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<Budget>> GetByYearAsync(int condominiumId, int year, IUser currentUser, CancellationToken cancellationToken);
        Task<Budget> GetCurrentBudgetAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> ApproveBudgetAsync(int budgetId, long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> CloseBudgetAsync(int budgetId, long userId, IUser currentUser, CancellationToken cancellationToken);
    }
}
