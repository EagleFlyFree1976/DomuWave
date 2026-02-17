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
    public class SupplierContractService : BaseService<SupplierContract, int>, ISupplierContractService
    {
        public SupplierContractService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "SupplierContracts";

        public async Task<IList<SupplierContract>> GetByCondominiumIdAsync(int condominiumId, CancellationToken cancellationToken = default)
        {
            return await session.Query<SupplierContract>()
                .Where(c => c.CondominiumId == condominiumId && !c.IsDeleted)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<SupplierContract>> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            return await session.Query<SupplierContract>()
                .Where(c => c.SupplierId == supplierId && !c.IsDeleted)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<SupplierContract>> GetActiveContractsAsync(int condominiumId, CancellationToken cancellationToken = default)
        {
            return await session.Query<SupplierContract>()
                .Where(c => c.CondominiumId == condominiumId 
                         && c.Status == "Active" 
                         && !c.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<SupplierContract>> GetExpiringContractsAsync(int condominiumId, int daysAhead, CancellationToken cancellationToken = default)
        {
            var targetDate = DateTime.UtcNow.AddDays(daysAhead);
            
            return await session.Query<SupplierContract>()
                .Where(c => c.CondominiumId == condominiumId
                         && c.Status == "Active"
                         && !c.IsDeleted
                         && c.EndDate.HasValue
                         && c.EndDate.Value <= targetDate)
                .OrderBy(c => c.EndDate)
                .ToListAsync(cancellationToken);
        }
    }
}
