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
    public class SupplierService : BaseService<Supplier, int>, ISupplierService
    {
        public SupplierService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }
        
        public override string CacheRegion => "Suppliers";

        public async Task<IList<Supplier>> GetByTypeAsync(Guid tenantId, string supplierType, CancellationToken cancellationToken = default)
        {
            return await session.Query<Supplier>()
                .Where(s => s.TenantId == tenantId 
                         && s.SupplierType == supplierType 
                         && !s.IsDeleted)
                .OrderBy(s => s.CompanyName)
                .ToListAsync(cancellationToken);
        }

        public async Task<Supplier> GetByVatNumberAsync(Guid tenantId, string vatNumber, CancellationToken cancellationToken = default)
        {
            return await session.Query<Supplier>()
                .Where(s => s.TenantId == tenantId 
                         && s.VatNumber == vatNumber 
                         && !s.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<Supplier>> SearchSuppliersAsync(Guid tenantId, string searchTerm, CancellationToken cancellationToken = default)
        {
            var term = searchTerm.ToLower();
            
            return await session.Query<Supplier>()
                .Where(s => s.TenantId == tenantId 
                         && !s.IsDeleted
                         && (s.CompanyName.ToLower().Contains(term)
                             || s.Email.ToLower().Contains(term)
                             || s.ContactPerson.ToLower().Contains(term)))
                .OrderBy(s => s.CompanyName)
                .ToListAsync(cancellationToken);
        }
    }
}
