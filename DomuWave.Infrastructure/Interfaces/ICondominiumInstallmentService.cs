using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface ICondominiumInstallmentService : IBaseService<CondominiumInstallment, int>
    {
        Task<IList<CondominiumInstallment>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<CondominiumInstallment>> GetByYearAsync(int condominiumId, int year,  User currentUser, CancellationToken cancellationToken);
        Task<CondominiumInstallment> GetByYearAndNumberAsync(int condominiumId, int year, int installmentNumber, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<CondominiumInstallment>> GetOpenInstallmentsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<CondominiumInstallment>> GetOverdueInstallmentsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<bool> GenerateInstallmentsAsync(int condominiumId, int year, int budgetId, long userId, IUser currentUser, CancellationToken cancellationToken);
    }
}
