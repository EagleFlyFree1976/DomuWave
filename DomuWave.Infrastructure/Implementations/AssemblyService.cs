using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class AssemblyService : BaseService, IAssemblyService
{
    public AssemblyService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "Assemblies";

    public async Task<Assembly?> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Assembly>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<Assembly>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Assembly>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<Assembly>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Assembly>()
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<Assembly>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Assembly>()
            .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
            .OrderByDescending(x => x.ScheduledDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Assembly?> GetWithDetailsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Assembly>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<Assembly>> FindAsync(Expression<Func<Assembly, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Assembly>()
            .Where(predicate)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Assembly> CreateAsync(Assembly entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<Assembly> UpdateAsync(Assembly entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<Assembly>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return false;
        entity.Trace(currentUser);
        entity.IsDeleted = true;
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<Assembly>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<Assembly, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Assembly>()
            .Where(x => !x.IsDeleted)
            .CountAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Assembly>()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<(IList<Assembly> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<Assembly, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<Assembly, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<Assembly>()
            .Where(x => !x.IsDeleted)
            .Where(filter);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return (items, totalCount);
    }
}
