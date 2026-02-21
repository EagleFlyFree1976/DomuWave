using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Domain.Models;
using NHibernate.Linq;

using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations
{
    public class CommunicationReadService : BaseService, ICommunicationReadService
    {
        public CommunicationReadService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) 
            : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "CommunicationReads";

        public async Task<CommunicationRead> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CommunicationRead>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<CommunicationRead>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CommunicationRead>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CommunicationRead>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CommunicationRead>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<CommunicationRead>> FindAsync(Expression<Func<CommunicationRead, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CommunicationRead>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<CommunicationRead> CreateAsync(CommunicationRead entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<CommunicationRead> UpdateAsync(CommunicationRead entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var communicationRead = await session.Query<CommunicationRead>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (communicationRead == null)
                return false;

            communicationRead.Trace(currentUser);
            communicationRead.IsDeleted = true;
            await session.SaveOrUpdateAsync(communicationRead, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var communicationRead = await session.Query<CommunicationRead>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (communicationRead == null)
                return false;

            await session.DeleteAsync(communicationRead, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<CommunicationRead, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CommunicationRead>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CommunicationRead>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<CommunicationRead> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<CommunicationRead, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<CommunicationRead, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<CommunicationRead>()
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

        public async Task<IList<CommunicationRead>> GetByCommunicationIdAsync(int communicationId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<CommunicationRead>()
                .Where(x => x.Communication.Id == communicationId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> MarkAsReadAsync(int communicationId, long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            var communicationRead = await session.Query<CommunicationRead>()
                .FirstOrDefaultAsync(x => x.Communication.Id == communicationId && x.UserId == userId, cancellationToken);

            if (communicationRead != null)
                return true;

            var communication = await session.Query<Communication>()
                .FirstOrDefaultAsync(x => x.Id == communicationId, cancellationToken);

            if (communication == null)
                return false;

            var newRead = new CommunicationRead
            {
                Communication = communication,
                UserId = userId,
                ReadDate = DateTime.Now
            };

            newRead.Trace(currentUser);
            await session.SaveOrUpdateAsync(newRead, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> GetUnreadCountAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            var readCommunicationIds = await session.Query<CommunicationRead>()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .Select(x => x.Communication.Id)
                .ToListAsync(cancellationToken);

            return await session.Query<Communication>()
                .Where(x => !readCommunicationIds.Contains(x.Id) && !x.IsDeleted)
                .CountAsync(cancellationToken);
        }
    }
}
