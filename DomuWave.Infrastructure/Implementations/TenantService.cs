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
    public class TenantService : BaseService<Tenant, Guid>, ITenantService
    {
        public TenantService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "Tenants";

        public async Task<Tenant> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await session.Query<Tenant>()
                .Where(t => t.Code == code && !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
        {
            return await session.Query<Tenant>()
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
