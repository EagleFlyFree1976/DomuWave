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
    public class CondominiumService : BaseService<Condominium, int>, ICondominiumService
    {
        public CondominiumService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "Condominiums";

        public async Task<Condominium> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
        {
            return await session.Query<Condominium>()
                .Where(c => c.TenantId == tenantId && c.Code == code && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<Condominium>> GetActiveCondominiumsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return await session.Query<Condominium>()
                .Where(c => c.TenantId == tenantId && c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Condominium>> GetCondominiumsWithUpcomingAssemblyAsync(Guid tenantId, int daysAhead, CancellationToken cancellationToken = default)
        {
            var targetDate = DateTime.UtcNow.AddDays(daysAhead);
            
            return await session.Query<Condominium>()
                .Where(c => c.TenantId == tenantId 
                         && c.IsActive 
                         && !c.IsDeleted
                         && c.LastAssemblyDate.HasValue
                         && c.LastAssemblyDate.Value.AddYears(1) <= targetDate)
                .OrderBy(c => c.LastAssemblyDate)
                .ToListAsync(cancellationToken);
        }
    }
}
