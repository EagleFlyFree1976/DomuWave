using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumInstallmentService : IBaseService<CondominiumInstallment, int>
    {
        Task<IList<CondominiumInstallment>> GetByCondominiumIdAsync(int condominiumId);
        Task<IList<CondominiumInstallment>> GetByYearAsync(int condominiumId, int year);
        Task<CondominiumInstallment> GetByYearAndNumberAsync(int condominiumId, int year, int installmentNumber);
        Task<IList<CondominiumInstallment>> GetOpenInstallmentsAsync(int condominiumId);
        Task<IList<CondominiumInstallment>> GetOverdueInstallmentsAsync(int condominiumId);
        Task<bool> GenerateInstallmentsAsync(int condominiumId, int year, int budgetId, long userId);
    }
}
