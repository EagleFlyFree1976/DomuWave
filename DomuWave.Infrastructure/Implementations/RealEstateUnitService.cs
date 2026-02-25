using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Models;
using NHibernate.Linq;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class RealEstateUnitService: BaseService, IRealEstateUnitService
    {
        public RealEstateUnitService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "RealEstateUnits";

        public async Task<RealEstateUnit> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> FindAsync(Expression<Func<RealEstateUnit, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<RealEstateUnit> CreateAsync(RealEstateUnit entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<RealEstateUnit> UpdateAsync(RealEstateUnit entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var unit = await session.Query<RealEstateUnit>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (unit == null)
                return false;

            unit.Trace(currentUser);
            unit.IsDeleted = true;
            await session.SaveOrUpdateAsync(unit, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var unit = await session.Query<RealEstateUnit>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (unit == null)
                return false;

            await session.DeleteAsync(unit, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<RealEstateUnit, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<RealEstateUnit> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<RealEstateUnit, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<RealEstateUnit, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<RealEstateUnit>()
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

        public async Task<IList<RealEstateUnit>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .OrderBy(x => x.Staircase)
                .ThenBy(x => x.Floor)
                .ThenBy(x => x.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> GetByStaircaseAsync(int condominiumId, string staircase, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.Staircase == staircase 
                    && !x.IsDeleted)
                .OrderBy(x => x.Floor)
                .ThenBy(x => x.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> GetByFloorAsync(int condominiumId, int floor, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.Floor == floor 
                    && !x.IsDeleted)
                .OrderBy(x => x.Staircase)
                .ThenBy(x => x.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<RealEstateUnit>> GetByTypeAsync(int condominiumId, string unitType, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.UnitType == unitType 
                    && !x.IsDeleted)
                .OrderBy(x => x.Staircase)
                .ThenBy(x => x.Floor)
                .ThenBy(x => x.InternalNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalUnitsCountAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<RealEstateUnit>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .CountAsync(cancellationToken);
        }
    }
}
