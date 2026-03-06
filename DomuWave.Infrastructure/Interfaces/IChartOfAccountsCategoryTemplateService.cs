using CPQ.Core.Memberships;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

public interface IChartOfAccountsCategoryTemplateService : IBaseService<ChartOfAccountsCategoryTemplate, int>
{
    Task<IList<ChartOfAccountsCategoryTemplate>> GetActiveAsync(IUser currentUser, CancellationToken cancellationToken);
}
