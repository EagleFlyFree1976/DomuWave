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
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class UnitMillesimalService : BaseService, IUnitMillesimalService
    {
        public UnitMillesimalService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "UnitMillesimals";

        public async Task<UnitMillesimal> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<UnitMillesimal>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitMillesimal>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitMillesimal>> FindAsync(Expression<Func<UnitMillesimal, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<UnitMillesimal> CreateAsync(UnitMillesimal entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<UnitMillesimal> UpdateAsync(UnitMillesimal entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var unitMillesimal = await session.Query<UnitMillesimal>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (unitMillesimal == null)
                return false;

            unitMillesimal.Trace(currentUser);
            unitMillesimal.IsDeleted = true;
            await session.SaveOrUpdateAsync(unitMillesimal, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var unitMillesimal = await session.Query<UnitMillesimal>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (unitMillesimal == null)
                return false;

            await session.DeleteAsync(unitMillesimal, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<UnitMillesimal, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<UnitMillesimal> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<UnitMillesimal, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<UnitMillesimal, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<UnitMillesimal>()
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

        public async Task<IList<UnitMillesimal>> GetByMillesimalTableIdAsync(int millesimalTableId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .Where(x => x.MillesimalTable.Id == millesimalTableId && !x.IsDeleted)
                .OrderBy(x => x.Unit.Staircase)
                .ThenBy(x => x.Unit.Floor)
                .ThenBy(x => x.Unit.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UnitMillesimal>> GetByUnitIdAsync(int unitId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .Where(x => x.Unit.Id == unitId && !x.IsDeleted)
                .OrderByDescending(x => x.MillesimalTable.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<UnitMillesimal> GetByTableAndUnitAsync(int millesimalTableId, int unitId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .FirstOrDefaultAsync(x => x.MillesimalTable.Id == millesimalTableId 
                    && x.Unit.Id == unitId 
                    && !x.IsDeleted, cancellationToken);
        }

        public async Task<decimal> GetTotalMillesimalAsync(int millesimalTableId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<UnitMillesimal>()
                .Where(x => x.MillesimalTable.Id == millesimalTableId && !x.IsDeleted)
                .SumAsync(x => x.Millesimal, cancellationToken);
        }

        public async Task<bool> ValidateTotalMillesimalAsync(int millesimalTableId, decimal expectedTotal, IUser currentUser,
            CancellationToken cancellationToken)
        {
            var actualTotal = await GetTotalMillesimalAsync(millesimalTableId, currentUser, cancellationToken);
            return actualTotal == expectedTotal;
        }
    }
}
