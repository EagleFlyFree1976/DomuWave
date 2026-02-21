using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using NHibernate.Linq;
using DomuWave.Domain.Models;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class SupplierService : BaseService, ISupplierService
    {
        public SupplierService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "Suppliers";

        public async Task<Supplier> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Supplier>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Supplier>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Supplier>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Supplier>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Supplier>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Supplier>> FindAsync(Expression<Func<Supplier, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Supplier>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<Supplier> CreateAsync(Supplier entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Supplier> UpdateAsync(Supplier entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var supplier = await session.Query<Supplier>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (supplier == null)
                return false;

            supplier.Trace(currentUser);
            supplier.IsDeleted = true;
            await session.SaveOrUpdateAsync(supplier, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var supplier = await session.Query<Supplier>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (supplier == null)
                return false;

            await session.DeleteAsync(supplier, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Supplier, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Supplier>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Supplier>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<Supplier> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<Supplier, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<Supplier, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Supplier>()
                .Where(x => !x.IsDeleted)
                .Where(filter);

            var totalCount = await query.CountAsync(cancellationToken);

            if (ascending)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(orderBy);
            }

            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var items = await query.ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<IList<Supplier>> GetByTypeAsync(Guid tenantId, string supplierType, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Supplier>()
                .Where(x => x.Tenant.Id == tenantId 
                    && x.SupplierType == supplierType 
                    && !x.IsDeleted)
                .OrderBy(x => x.CompanyName)
                .ToListAsync(cancellationToken);
        }

        public async Task<Supplier> GetByVatNumberAsync(Guid tenantId, string vatNumber, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Supplier>()
                .FirstOrDefaultAsync(x => x.Tenant.Id == tenantId 
                    && x.VatNumber == vatNumber 
                    && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Supplier>> SearchSuppliersAsync(Guid tenantId, string searchTerm, IUser currentUser, CancellationToken cancellationToken)
        {
            var lowerSearchTerm = searchTerm.ToLower();

            return await session.Query<Supplier>()
                .Where(x => x.Tenant.Id == tenantId 
                    && !x.IsDeleted
                    && (x.CompanyName.ToLower().Contains(lowerSearchTerm)
                        || x.VatNumber.ToLower().Contains(lowerSearchTerm)
                        || x.Email.ToLower().Contains(lowerSearchTerm)
                        || x.Phone.ToLower().Contains(lowerSearchTerm)
                        || x.ContactPerson.ToLower().Contains(lowerSearchTerm)))
                .OrderBy(x => x.CompanyName)
                .ToListAsync(cancellationToken);
        }
    }
}
