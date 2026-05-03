using System.Collections.Generic;
using System.Threading.Tasks;
using CPQ.Core.Memberships;
using DomuWave.Services.Models;
using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces
{
    public interface IUnitOwnerService : IBaseService<UnitOwner, int>
    {
        Task<IList<UnitOwner>> GetByUnitIdAsync(int unitId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<UnitOwner>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken cancellationToken);
        Task<IList<UnitOwner>> GetActiveOwnersAsync(int unitId, IUser currentUser, CancellationToken cancellationToken);
        Task<decimal> GetTotalOwnershipQuotaAsync(int unitId, IUser currentUser, CancellationToken cancellationToken);
    }
}
