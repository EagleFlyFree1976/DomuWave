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
    public class CondominiumCadastralDataService : BaseService, ICondominiumCadastralDataService
    {
        public CondominiumCadastralDataService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) :
            base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "CondominiumCadastralData";

        public async Task<CondominiumCadastralData> GetByIdAsync(int id, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumCadastralData>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<CondominiumCadastralData>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumCadastralData>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumCadastralData>> GetByTenantIdAsync(Guid tenantId, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumCadastralData>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CondominiumCadastralData>> FindAsync(
            Expression<Func<CondominiumCadastralData, bool>> predicate, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumCadastralData>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<CondominiumCadastralData> CreateAsync(CondominiumCadastralData entity, IUser currentUser,
            CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<CondominiumCadastralData> UpdateAsync(CondominiumCadastralData entity, IUser currentUser,
            CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var cadastralData = await session.Query<CondominiumCadastralData>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (cadastralData == null)
                return false;

            cadastralData.Trace(currentUser);
            cadastralData.IsDeleted = true;
            await session.SaveOrUpdateAsync(cadastralData, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var cadastralData = await session.Query<CondominiumCadastralData>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (cadastralData == null)
                return false;

            await session.DeleteAsync(cadastralData, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<CondominiumCadastralData, bool>> predicate, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumCadastralData>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumCadastralData>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<CondominiumCadastralData> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<CondominiumCadastralData, bool>> filter,
            int pageNumber, int pageSize,
            Expression<Func<CondominiumCadastralData, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<CondominiumCadastralData>()
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

        public async Task<CondominiumCadastralData> GetByCondominiumIdAsync(int condominiumId, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<CondominiumCadastralData>()
                .FirstOrDefaultAsync(x => x.Condominium.Id == condominiumId && !x.IsDeleted, cancellationToken);
        }
    }
}