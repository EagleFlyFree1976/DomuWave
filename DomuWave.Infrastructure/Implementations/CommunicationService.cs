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
    public class CommunicationService : BaseService, ICommunicationService
    {
        public CommunicationService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "Communications";

        public async Task<Communication> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Communication>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Communication>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Communication>> FindAsync(Expression<Func<Communication, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => !x.IsDeleted)
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Communication> CreateAsync(Communication entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Communication> UpdateAsync(Communication entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var entity = await session.Query<Communication>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
            
            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var entity = await session.Query<Communication>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            if (entity == null)
                return false;

            await session.DeleteAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Communication, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => !x.IsDeleted)
                .Where(predicate)
                .CountAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => x.Id == id && !x.IsDeleted)
                .AnyAsync(cancellationToken);
        }

        public async Task<(IList<Communication> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Communication, bool>> filter, Expression<Func<Communication, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Communication>()
                .Where(x => !x.IsDeleted)
                .Where(filter);

            var totalCount = await query.CountAsync(cancellationToken);
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            var items = await query
                
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<IList<Communication>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            
            return await session.Query<Communication>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Communication>> GetVisibleCommunicationsAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted && x.IsVisible)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Communication>> GetByTypeAsync(int condominiumId, string communicationType, IUser currentUser,
            CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => x.Condominium.Id == condominiumId && x.CommunicationType == communicationType && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Communication>> GetByPriorityAsync(int condominiumId, string priority, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => x.Condominium.Id == condominiumId && x.Priority == priority && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Communication>> GetUnreadByUserAsync(int condominiumId, long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Communication>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted && x.IsVisible)
                .Where(x => !x.Reads.Any(r => r.UserId == userId))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> PublishCommunicationAsync(int communicationId, long userId, IUser currentUser,
            CancellationToken cancellationToken)
        {
            var entity = await session.Query<Communication>()
                .FirstOrDefaultAsync(x => x.Id == communicationId && !x.IsDeleted, cancellationToken);
            
            if (entity == null)
                return false;

            entity.IsVisible = true;
            entity.PublicationDate = DateTime.UtcNow;
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }
    }
}
