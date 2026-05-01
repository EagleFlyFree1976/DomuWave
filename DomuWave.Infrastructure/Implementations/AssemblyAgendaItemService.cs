using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class AssemblyAgendaItemService : BaseService, IAssemblyAgendaItemService
{
    public AssemblyAgendaItemService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "AssemblyAgendaItems";

    public async Task<AssemblyAgendaItem?> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAgendaItem>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<AssemblyAgendaItem>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAgendaItem>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<AssemblyAgendaItem>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAgendaItem>()
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<AssemblyAgendaItem>> GetByAssemblyIdAsync(int assemblyId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAgendaItem>()
            .Where(x => x.Assembly.Id == assemblyId && !x.IsDeleted)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<AssemblyAgendaItem>> FindAsync(Expression<Func<AssemblyAgendaItem, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAgendaItem>()
            .Where(predicate)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<AssemblyAgendaItem> CreateAsync(AssemblyAgendaItem entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<AssemblyAgendaItem> UpdateAsync(AssemblyAgendaItem entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<AssemblyAgendaItem>()
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
        var entity = await session.Query<AssemblyAgendaItem>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<AssemblyAgendaItem, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAgendaItem>()
            .Where(x => !x.IsDeleted)
            .CountAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAgendaItem>()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<(IList<AssemblyAgendaItem> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<AssemblyAgendaItem, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<AssemblyAgendaItem, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<AssemblyAgendaItem>()
            .Where(x => !x.IsDeleted)
            .Where(filter);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return (items, totalCount);
    }
}
