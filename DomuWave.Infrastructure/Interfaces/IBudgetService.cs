using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IBudgetService : IBaseService<Budget, int>
    {
        Task<IList<Budget>> GetByCondominiumIdAsync(int condominiumId);
        Task<Budget> GetByYearAndTypeAsync(int condominiumId, int year, string type);
        Task<IList<Budget>> GetByYearAsync(int condominiumId, int year);
        Task<Budget> GetCurrentBudgetAsync(int condominiumId);
        Task<bool> ApproveBudgetAsync(int budgetId, long userId);
        Task<bool> CloseBudgetAsync(int budgetId, long userId);
    }
}
