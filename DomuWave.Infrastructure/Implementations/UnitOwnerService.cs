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
    public class UnitOwnerService : BaseService, IUnitOwnerService
    {
        public UnitOwnerService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "UnitOwners";

        public async Task<UnitOwner> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<UnitOwner>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitOwner>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitOwner>> FindAsync(Expression<Func<UnitOwner, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<UnitOwner> CreateAsync(UnitOwner entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<UnitOwner> UpdateAsync(UnitOwner entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var unitOwner = await session.Query<UnitOwner>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (unitOwner == null)
                return false;

            unitOwner.Trace(currentUser);
            unitOwner.IsDeleted = true;
            await session.SaveOrUpdateAsync(unitOwner, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var unitOwner = await session.Query<UnitOwner>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (unitOwner == null)
                return false;

            await session.DeleteAsync(unitOwner, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<UnitOwner, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<UnitOwner> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<UnitOwner, bool>> filter, Expression<Func<UnitOwner, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<UnitOwner>()
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

        public async Task<IList<UnitOwner>> GetByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .Where(x => x.Unit.Id == unitId && !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitOwner>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitOwner>> GetActiveOwnersAsync(int unitId, IUser currentUser, CancellationToken cancellationToken)
        {
            var today = DateTime.Now;
            return await session.Query<UnitOwner>()
                .Where(x => x.Unit.Id == unitId 
                    && x.IsActive 
                    && x.StartDate <= today 
                    && (x.EndDate == null || x.EndDate >= today) 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalOwnershipQuotaAsync(int unitId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitOwner>()
                .Where(x => x.Unit.Id == unitId && !x.IsDeleted)
                .SumAsync(x => x.OwnershipQuota, cancellationToken);
        }
    }
}
