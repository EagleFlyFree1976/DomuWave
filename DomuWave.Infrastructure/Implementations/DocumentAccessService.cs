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
    public class DocumentAccessService : BaseService, IDocumentAccessService
    {
        public DocumentAccessService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "DocumentAccess";

        public async Task<DocumentAccess> GetByIdAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<DocumentAccess>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<DocumentAccess>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<DocumentAccess>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<DocumentAccess>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<DocumentAccess>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<DocumentAccess>> FindAsync(Expression<Func<DocumentAccess, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<DocumentAccess>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<DocumentAccess> CreateAsync(DocumentAccess entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<DocumentAccess> UpdateAsync(DocumentAccess entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var documentAccess = await session.Query<DocumentAccess>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (documentAccess == null)
                return false;

            documentAccess.Trace(currentUser);
            documentAccess.IsDeleted = true;
            await session.SaveOrUpdateAsync(documentAccess, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            var documentAccess = await session.Query<DocumentAccess>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (documentAccess == null)
                return false;

            await session.DeleteAsync(documentAccess, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<DocumentAccess, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<DocumentAccess>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(long id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<DocumentAccess>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<DocumentAccess> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<DocumentAccess, bool>> filter, int pageNumber, int pageSize,
            Expression<Func<DocumentAccess, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<DocumentAccess>()
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

        public async Task<IList<DocumentAccess>> GetByDocumentIdAsync(int documentId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<DocumentAccess>()
                .Where(x => x.Document.Id == documentId && !x.IsDeleted)
                .OrderByDescending(x => x.AccessDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<DocumentAccess>> GetByUserIdAsync(long userId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<DocumentAccess>()
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.AccessDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> LogAccessAsync(int documentId, long userId, string accessType, string ipAddress, IUser currentUser,
            CancellationToken cancellationToken)
        {
            var document = await session.Query<Document>()
                .FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);

            if (document == null)
                return false;

            var documentAccess = new DocumentAccess
            {
                Document = document,
                UserId = userId,
                AccessDate = DateTime.UtcNow,
                AccessType = accessType,
                IpAddress = ipAddress
            };

            documentAccess.Trace(currentUser);
            await session.SaveOrUpdateAsync(documentAccess, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }
    }
}
