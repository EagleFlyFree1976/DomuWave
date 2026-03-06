using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IChartOfAccountsCategoryService : IBaseService<ChartOfAccountsCategory, int>
{
    Task<IList<ChartOfAccountsCategory>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
}
