using System;
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
    public class UnitTenantService : BaseService<UnitTenant, int>, IUnitTenantService
    {
        public UnitTenantService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "UnitTenants";

        public async Task<IList<UnitTenant>> GetByUnitIdAsync(int unitId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitTenant>()
                .Where(t => t.UnitId == unitId && !t.IsDeleted)
                .OrderByDescending(t => t.LeaseStartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<UnitTenant> GetActiveByUnitIdAsync(int unitId, CancellationToken cancellationToken = default)
        {
            return await session.Query<UnitTenant>()
                .Where(t => t.UnitId == unitId && t.IsActive && !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<UnitTenant>> GetExpiringLeasesAsync(Guid tenantId, int daysAhead, CancellationToken cancellationToken = default)
        {
            var targetDate = DateTime.UtcNow.AddDays(daysAhead);
            
            return await session.Query<UnitTenant>()
                .Where(t => t.OwnerTenantId == tenantId
                         && t.IsActive
                         && !t.IsDeleted
                         && t.LeaseEndDate.HasValue
                         && t.LeaseEndDate.Value <= targetDate)
                .OrderBy(t => t.LeaseEndDate)
                .ToListAsync(cancellationToken);
        }
    }
}
