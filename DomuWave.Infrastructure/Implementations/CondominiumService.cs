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
    public class CondominiumService : BaseService, ICondominiumService
    {
            public CondominiumService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "Condominiums";

        public async Task<Condominium> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<Condominium> GetByIdAsync(int id, Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .FirstOrDefaultAsync(x => x.Id == id && x.Tenant.Id == tenantId && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Condominium>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Condominium>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Condominium>> FindAsync(Expression<Func<Condominium, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<Condominium> CreateAsync(Condominium entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Condominium> UpdateAsync(Condominium entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var condominium = await session.Query<Condominium>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (condominium == null)
                return false;

            condominium.Trace(currentUser);
            condominium.IsDeleted = true;
            await session.SaveOrUpdateAsync(condominium, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var condominium = await session.Query<Condominium>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (condominium == null)
                return false;

            await session.DeleteAsync(condominium, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Condominium, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<Condominium> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<Condominium, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<Condominium, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Condominium>()
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

        public async Task<Condominium> GetByCodeAsync(Guid tenantId, string code, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .FirstOrDefaultAsync(x => x.Tenant.Id == tenantId 
                    && x.Code == code 
                    && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Condominium>> GetActiveCondominiumsAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Condominium>()
                .Where(x => x.Tenant.Id == tenantId 
                    && x.IsActive 
                    && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Condominium>> GetCondominiumsWithUpcomingAssemblyAsync(Guid tenantId, int daysAhead, IUser currentUser,
            CancellationToken cancellationToken)
        {
            var today = DateTime.Now;
            var targetDate = today.AddDays(daysAhead);

            return await session.Query<Condominium>()
                .Where(x => x.Tenant.Id == tenantId 
                    && !x.IsDeleted
                    && x.LastAssemblyDate.HasValue
                    && x.LastAssemblyDate.Value >= today
                    && x.LastAssemblyDate.Value <= targetDate)
                .ToListAsync(cancellationToken);
        }
    }
}
