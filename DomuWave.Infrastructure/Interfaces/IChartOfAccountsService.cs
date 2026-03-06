using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IChartOfAccountsService : IBaseService<ChartOfAccounts, int>
    {
        Task<IList<ChartOfAccounts>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<ChartOfAccounts> GetByCodeAsync(int condominiumId, string code, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<ChartOfAccounts>> GetByTypeAsync(int condominiumId, ChartOfAccountsType type, IUser currentUser,
            CancellationToken cancellationToken);
        Task<IList<ChartOfAccounts>> GetRootAccountsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<ChartOfAccounts>> GetChildAccountsAsync(int parentAccountId, IUser currentUser, CancellationToken cancellationToken);
    }
}
