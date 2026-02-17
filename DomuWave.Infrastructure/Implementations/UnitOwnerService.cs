using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;
using DomuWave.Common;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class UnitOwnerService : BaseService<UnitOwner, int>, IUnitOwnerService
    {
        public UnitOwnerService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "UnitOwners";

        public async Task<IList<UnitOwner>> GetByUnitIdAsync(int unitId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitOwner>()
                .Where(o => o.UnitId == unitId && !o.IsDeleted)
                .OrderBy(o => o.OwnershipQuota)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitOwner>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitOwner>()
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitOwner>> GetActiveOwnersAsync(int unitId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitOwner>()
                .Where(o => o.UnitId == unitId && o.IsActive && !o.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalOwnershipQuotaAsync(int unitId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitOwner>()
                .Where(o => o.UnitId == unitId && o.IsActive && !o.IsDeleted)
                .SumAsync(o => o.OwnershipQuota, cancellationToken);
        }
    }
}
