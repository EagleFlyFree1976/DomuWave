using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class ChartOfAccountsCategoryService : BaseService, IChartOfAccountsCategoryService
{
    public ChartOfAccountsCategoryService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "ChartOfAccountsCategory";

    public async Task<ChartOfAccountsCategory> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategory>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IList<ChartOfAccountsCategory>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategory>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<IList<ChartOfAccountsCategory>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategory>()
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IList<ChartOfAccountsCategory>> FindAsync(Expression<Func<ChartOfAccountsCategory, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategory>()
            .Where(predicate)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<ChartOfAccountsCategory> CreateAsync(ChartOfAccountsCategory entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<ChartOfAccountsCategory> UpdateAsync(ChartOfAccountsCategory entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<ChartOfAccountsCategory>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        entity.Trace(currentUser);
        entity.IsDeleted = true;
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<ChartOfAccountsCategory>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<ChartOfAccountsCategory, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategory>()
            .Where(x => !x.IsDeleted)
            .CountAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategory>()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<(IList<ChartOfAccountsCategory> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<ChartOfAccountsCategory, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<ChartOfAccountsCategory, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<ChartOfAccountsCategory>()
            .Where(x => !x.IsDeleted)
            .Where(filter);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);

        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
