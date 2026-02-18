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
    public class DocumentService : BaseService, IDocumentService
    {
        public DocumentService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache)
        {
        }

        public override string CacheRegion => "Documents";

        public async Task<Document> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<IList<Document>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Document>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Document>> FindAsync(Expression<Func<Document, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .Where(predicate)
                .Where(x => !x.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<Document> CreateAsync(Document entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<Document> UpdateAsync(Document entity, IUser currentUser, CancellationToken cancellationToken)
        {
            entity.Trace(currentUser);
            await session.SaveOrUpdateAsync(entity, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return entity;
        }

        public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var document = await session.Query<Document>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (document == null)
                return false;

            document.Trace(currentUser);
            document.IsDeleted = true;
            await session.SaveOrUpdateAsync(document, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            var document = await session.Query<Document>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (document == null)
                return false;

            await session.DeleteAsync(document, cancellationToken);
            await session.FlushAsync(cancellationToken);
            return true;
        }

        public async Task<int> CountAsync(Expression<Func<Document, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .Where(x => !x.IsDeleted)
                .CountAsync(predicate, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        }

        public async Task<(IList<Document> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Document, bool>> filter, Expression<Func<Document, object>> orderBy, bool ascending,
            IUser currentUser, CancellationToken cancellationToken)
        {
            var query = session.Query<Document>()
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

        public async Task<IList<Document>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Document>> GetByCategoryAsync(int condominiumId, string category, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.Category == category 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Document>> GetVisibleToOwnersAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && x.IsVisibleToOwners 
                    && !x.IsArchived 
                    && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Document>> SearchDocumentsAsync(int condominiumId, string searchTerm, IUser currentUser, CancellationToken cancellationToken)
        {
            var lowerSearchTerm = searchTerm.ToLower();
            
            return await session.Query<Document>()
                .Where(x => x.Condominium.Id == condominiumId 
                    && !x.IsDeleted
                    && (x.Title.ToLower().Contains(lowerSearchTerm) 
                        || x.FileName.ToLower().Contains(lowerSearchTerm)
                        || x.Tags.ToLower().Contains(lowerSearchTerm)))
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Document>> GetRecentDocumentsAsync(int condominiumId, int count, IUser currentUser, CancellationToken cancellationToken)
        {
            return await session.Query<Document>()
                .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
                .OrderByDescending(x => x.CreationDate)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
