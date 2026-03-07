using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class WorkQuoteService : BaseService, IWorkQuoteService
{
    public WorkQuoteService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "WorkQuote";

    public async Task<WorkQuote> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuote>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IList<WorkQuote>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuote>().Where(x => !x.IsDeleted).ToListAsync(cancellationToken);

    public async Task<IList<WorkQuote>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuote>().Where(x => x.Tenant.Id == tenantId && !x.IsDeleted).ToListAsync(cancellationToken);

    public async Task<IList<WorkQuote>> FindAsync(Expression<Func<WorkQuote, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuote>().Where(predicate).Where(x => !x.IsDeleted).ToListAsync(cancellationToken);

    public async Task<WorkQuote> CreateAsync(WorkQuote entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<WorkQuote> UpdateAsync(WorkQuote entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<WorkQuote>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        entity.Trace(currentUser);
        entity.IsDeleted = true;
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<WorkQuote>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<WorkQuote, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuote>().Where(x => !x.IsDeleted).CountAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuote>().AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<(IList<WorkQuote> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<WorkQuote, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<WorkQuote, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<WorkQuote>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(cancellationToken);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return (await query.ToListAsync(cancellationToken), total);
    }

    public async Task<IList<WorkQuote>> GetByWorkIdAsync(int workId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<WorkQuote>()
            .Where(x => x.Work.Id == workId && !x.IsDeleted)
            .OrderByDescending(x => x.QuoteDate)
            .ToListAsync(cancellationToken);

    public async Task<WorkQuote?> ApproveAsync(int quoteId, IUser currentUser, CancellationToken cancellationToken)
    {
        var quote = await session.Query<WorkQuote>().FirstOrDefaultAsync(x => x.Id == quoteId && !x.IsDeleted, cancellationToken);
        if (quote == null) return null;

        // Reject all other quotes for the same work
        var others = await session.Query<WorkQuote>()
            .Where(x => x.Work.Id == quote.Work.Id && x.Id != quoteId && !x.IsDeleted)
            .ToListAsync(cancellationToken);
        foreach (var other in others)
        {
            other.IsApproved = false;
            other.Status     = "Rejected";
            other.Trace(currentUser);
            await session.SaveOrUpdateAsync(other, cancellationToken);
        }

        quote.IsApproved = true;
        quote.Status     = "Approved";
        quote.Trace(currentUser);
        await session.SaveOrUpdateAsync(quote, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return quote;
    }

    public async Task<WorkQuote?> RejectAsync(int quoteId, IUser currentUser, CancellationToken cancellationToken)
    {
        var quote = await session.Query<WorkQuote>().FirstOrDefaultAsync(x => x.Id == quoteId && !x.IsDeleted, cancellationToken);
        if (quote == null) return null;
        quote.IsApproved = false;
        quote.Status     = "Rejected";
        quote.Trace(currentUser);
        await session.SaveOrUpdateAsync(quote, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return quote;
    }
}
