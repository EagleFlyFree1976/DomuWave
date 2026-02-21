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
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class TenantService : BaseService, ITenantService
    {
        public TenantService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "Tenants";

        public async Task<Tenant> GetByIdAsync(Guid id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Tenant>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Tenant>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Tenant>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Tenant>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            // For Tenant service, this method returns the tenant itself filtered by ID
            return await session.Query<Tenant>()
                .Where(x => x.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Tenant>> FindAsync(Expression<Func<Tenant, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Tenant>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<Tenant> CreateAsync(Tenant entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Tenant> UpdateAsync(Tenant entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id, IUser currentUser, CancellationToken cancellationToken)
        {
            var tenant = await session.Query<Tenant>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (tenant == null)
                return false;

            tenant.Trace(currentUser);
            tenant.IsDeleted = true;
            await session.SaveOrUpdateAsync(tenant, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(Guid id, IUser currentUser, CancellationToken cancellationToken)
        {
            var tenant = await session.Query<Tenant>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (tenant == null)
                return false;

            await session.DeleteAsync(tenant, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Tenant, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Tenant>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Tenant>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<Tenant> Items, int TotalCount)> GetPagedAsync(Expression<Func<Tenant, bool>> filter,
            int pageNumber, int pageSize, Expression<Func<Tenant, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Tenant>()
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

        public async Task<Tenant> GetByCodeAsync(string code, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Tenant>()
                .FirstOrDefaultAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Tenant>> GetActiveTenantsAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Tenant>()
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
