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
    public class MessageService : BaseService, IMessageService
    {
        public MessageService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "Messages";

        public async Task<Message> GetByIdAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Message>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Message>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Message>> FindAsync(Expression<Func<Message, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<Message> CreateAsync(Message entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Message> UpdateAsync(Message entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var message = await session.Query<Message>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (message == null)
                return false;

            message.Trace(currentUser);
            message.IsDeleted = true;
            await session.SaveOrUpdateAsync(message, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var message = await session.Query<Message>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (message == null)
                return false;

            await session.DeleteAsync(message, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Message, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<Message> Items, int TotalCount)> GetPagedAsync(Expression<Func<Message, bool>> filter,
            int pageNumber, int pageSize, Expression<Func<Message, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Message>()
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

        public async Task<IList<Message>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Message>> GetBySenderIdAsync(long senderId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => x.SenderId == senderId && !x.IsDeleted && x.ParentMessage == null)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Message>> GetByRecipientIdAsync(long recipientId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => x.RecipientId == recipientId && !x.IsDeleted && x.ParentMessage == null)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Message>> GetConversationAsync(long userId1, long userId2, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => !x.IsDeleted
                    && ((x.SenderId == userId1 && x.RecipientId == userId2)
                        || (x.SenderId == userId2 && x.RecipientId == userId1)))
                .OrderBy(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Message>> GetUnreadMessagesAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => x.RecipientId == userId 
                    && !x.IsRead 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> MarkAsReadAsync(long messageId, IUser currentUser, CancellationToken cancellationToken)
        {
            var message = await session.Query<Message>()
                .FirstOrDefaultAsync(x => x.Id == messageId && !x.IsDeleted, cancellationToken);

            if (message == null)
                return false;

            if (message.IsRead)
                return true;

            message.IsRead = true;
            message.ReadDate = DateTime.UtcNow;
            message.Trace(currentUser);

            await session.SaveOrUpdateAsync(message, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> GetUnreadCountAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Message>()
                .Where(x => x.RecipientId == userId 
                    && !x.IsRead 
                    && !x.IsDeleted)
                .CountAsync(cancellationToken);
        }
    }
}
