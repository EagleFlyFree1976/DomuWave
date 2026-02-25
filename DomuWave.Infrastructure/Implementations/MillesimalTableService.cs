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
    public class MillesimalTableService : BaseService, IMillesimalTableService
    {
        public MillesimalTableService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "MillesimalTables";

        public async Task<MillesimalTable> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<MillesimalTable>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<MillesimalTable>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<MillesimalTable>> FindAsync(Expression<Func<MillesimalTable, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<MillesimalTable> CreateAsync(MillesimalTable entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<MillesimalTable> UpdateAsync(MillesimalTable entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var millesimalTable = await session.Query<MillesimalTable>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (millesimalTable == null)
                return false;

            millesimalTable.Trace(currentUser);
            millesimalTable.IsDeleted = true;
            await session.SaveOrUpdateAsync(millesimalTable, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var millesimalTable = await session.Query<MillesimalTable>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (millesimalTable == null)
                return false;

            await session.DeleteAsync(millesimalTable, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<MillesimalTable, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<MillesimalTable> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<MillesimalTable, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<MillesimalTable, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<MillesimalTable>()
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

        public async Task<IList<MillesimalTable>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<MillesimalTable> GetByCodeAsync(int condominiumId, string code, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .FirstOrDefaultAsync(x => x.Condominium.Id == condominiumId 
                    && x.Code == code 
                    && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<MillesimalTable>> GetActiveTablesAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<MillesimalTable>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.IsActive 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }
    }
}
