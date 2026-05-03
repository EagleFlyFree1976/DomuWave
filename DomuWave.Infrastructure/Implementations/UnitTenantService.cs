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
    public class UnitTenantService : BaseService, IUnitTenantService
    {
        public UnitTenantService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "UnitTenants";

        public async Task<UnitTenant> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitTenant>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<UnitTenant>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitTenant>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitTenant>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitTenant>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitTenant>> FindAsync(Expression<Func<UnitTenant, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitTenant>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<UnitTenant> CreateAsync(UnitTenant entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<UnitTenant> UpdateAsync(UnitTenant entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var unitTenant = await session.Query<UnitTenant>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (unitTenant == null)
                return false;

            unitTenant.Trace(currentUser);
            unitTenant.IsDeleted = true;
            await session.SaveOrUpdateAsync(unitTenant, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var unitTenant = await session.Query<UnitTenant>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (unitTenant == null)
                return false;

            await session.DeleteAsync(unitTenant, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<UnitTenant, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitTenant>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitTenant>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<UnitTenant> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<UnitTenant, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<UnitTenant, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<UnitTenant>()
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

        public async Task<IList<UnitTenant>> GetByUnitIdAsync(int unitId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitTenant>()
                .Where(x => x.Unit.Id == unitId && x.Tenant.Id == tenantId && !x.IsDeleted)
                .OrderByDescending(x => x.LeaseStartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<UnitTenant> GetActiveByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken)
        {
            var today = DateTime.Now;
            return await session.Query<UnitTenant>()
                .FirstOrDefaultAsync(x => x.Unit.Id == unitId 
                    && x.IsActive 
                    && x.LeaseStartDate <= today 
                    && (x.LeaseEndDate == null || x.LeaseEndDate >= today) 
                    && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<UnitTenant>> GetExpiringLeasesAsync(Guid tenantId, int daysAhead, IUser currentUser, CancellationToken cancellationToken)
        {
            var today = DateTime.Now;
            var targetDate = today.AddDays(daysAhead);

            return await session.Query<UnitTenant>()
                .Where(x => x.Tenant.Id == tenantId 
                    && x.LeaseEndDate.HasValue
                    && x.LeaseEndDate >= today
                    && x.LeaseEndDate <= targetDate 
                    && !x.IsDeleted)
                .OrderBy(x => x.LeaseEndDate)
                .ToListAsync(cancellationToken);
        }
    }
}
