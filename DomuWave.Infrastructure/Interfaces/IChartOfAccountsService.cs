using System.Collections.Generic;
using System.Threading.Tasks;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IChartOfAccountsService : IBaseService<ChartOfAccounts, int>
    {
        Task<IList<ChartOfAccounts>> GetByCondominiumIdAsync(int condominiumId);
        Task<ChartOfAccounts> GetByCodeAsync(int condominiumId, string code);
        Task<IList<ChartOfAccounts>> GetByTypeAsync(int condominiumId, string type);
        Task<IList<ChartOfAccounts>> GetRootAccountsAsync(int condominiumId);
        Task<IList<ChartOfAccounts>> GetChildAccountsAsync(int parentAccountId);
    }
}
