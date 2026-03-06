using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class ChartOfAccountsCategoryTemplateService : BaseService, IChartOfAccountsCategoryTemplateService
{
    public ChartOfAccountsCategoryTemplateService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "ChartOfAccountsCategoryTemplate";

    public async Task<ChartOfAccountsCategoryTemplate> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategoryTemplate>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IList<ChartOfAccountsCategoryTemplate>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategoryTemplate>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IList<ChartOfAccountsCategoryTemplate>> GetActiveAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategoryTemplate>()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IList<ChartOfAccountsCategoryTemplate>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await GetAllAsync(currentUser, cancellationToken);

    public async Task<IList<ChartOfAccountsCategoryTemplate>> FindAsync(Expression<Func<ChartOfAccountsCategoryTemplate, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategoryTemplate>()
            .Where(predicate)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<ChartOfAccountsCategoryTemplate> CreateAsync(ChartOfAccountsCategoryTemplate entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<ChartOfAccountsCategoryTemplate> UpdateAsync(ChartOfAccountsCategoryTemplate entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<ChartOfAccountsCategoryTemplate>()
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
        var entity = await session.Query<ChartOfAccountsCategoryTemplate>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<ChartOfAccountsCategoryTemplate, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategoryTemplate>()
            .Where(x => !x.IsDeleted)
            .CountAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ChartOfAccountsCategoryTemplate>()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<(IList<ChartOfAccountsCategoryTemplate> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<ChartOfAccountsCategoryTemplate, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<ChartOfAccountsCategoryTemplate, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<ChartOfAccountsCategoryTemplate>()
            .Where(x => !x.IsDeleted)
            .Where(filter);
        var totalCount = await query.CountAsync(cancellationToken);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
