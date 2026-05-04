using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class BuildingService : BaseService, IBuildingService
{
    public BuildingService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "Buildings";

    public async Task<Building> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Building>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<Building>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Building>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<Building>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Building>()
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<Building>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Building>()
            .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<Building>> FindAsync(Expression<Func<Building, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Building>()
            .Where(predicate)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<Building> CreateAsync(Building entity, IUser currentUser, CancellationToken cancellationToken)
    {
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<Building> UpdateAsync(Building entity, IUser currentUser, CancellationToken cancellationToken)
    {
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<Building>()
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
        var entity = await session.Query<Building>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<Building, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Building>()
            .Where(x => !x.IsDeleted)
            .CountAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Building>()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<(IList<Building> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<Building, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<Building, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<Building>()
            .Where(x => !x.IsDeleted)
            .Where(filter);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        return (items, totalCount);
    }
}
