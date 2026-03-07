using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class WorkQuoteDocumentService : BaseService, IWorkQuoteDocumentService
{
    public WorkQuoteDocumentService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "WorkQuoteDocument";

    public async Task<WorkQuoteDocument> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuoteDocument>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IList<WorkQuoteDocument>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuoteDocument>().Where(x => !x.IsDeleted).ToListAsync(cancellationToken);

    public async Task<IList<WorkQuoteDocument>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuoteDocument>().Where(x => x.Tenant.Id == tenantId && !x.IsDeleted).ToListAsync(cancellationToken);

    public async Task<IList<WorkQuoteDocument>> FindAsync(Expression<Func<WorkQuoteDocument, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuoteDocument>().Where(predicate).Where(x => !x.IsDeleted).ToListAsync(cancellationToken);

    public async Task<WorkQuoteDocument> CreateAsync(WorkQuoteDocument entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<WorkQuoteDocument> UpdateAsync(WorkQuoteDocument entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<WorkQuoteDocument>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        entity.Trace(currentUser);
        entity.IsDeleted = true;
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<WorkQuoteDocument>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<WorkQuoteDocument, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuoteDocument>().Where(x => !x.IsDeleted).CountAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuoteDocument>().AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<(IList<WorkQuoteDocument> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<WorkQuoteDocument, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<WorkQuoteDocument, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<WorkQuoteDocument>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(cancellationToken);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return (await query.ToListAsync(cancellationToken), total);
    }

    public async Task<IList<WorkQuoteDocument>> GetByQuoteIdAsync(int quoteId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuoteDocument>()
            .Where(x => x.Work.Id == quoteId && !x.IsDeleted)
            .OrderBy(x => x.CreationDate)
            .ToListAsync(cancellationToken);
}
