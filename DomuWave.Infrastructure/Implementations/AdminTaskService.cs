using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class AdminTaskService : BaseService, IAdminTaskService
{
    public AdminTaskService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "AdminTask";

    public async Task<AdminTask> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AdminTask>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<AdminTask?> GetByIdWithCondominiumsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AdminTask>()
            .FetchMany(x => x.Condominiums).ThenFetch(c => c.Condominium)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IList<AdminTask>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AdminTask>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<IList<AdminTask>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AdminTask>()
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<IList<AdminTask>> GetByTenantFilteredAsync(
        Guid tenantId, int? assignedToUserId, int? statusId, DateTime? dueBefore,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<AdminTask>()
            .FetchMany(x => x.Condominiums).ThenFetch(c => c.Condominium)
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted);

        if (assignedToUserId.HasValue)
            query = query.Where(x => x.AssignedToUserId == assignedToUserId.Value);
        if (statusId.HasValue)
            query = query.Where(x => x.Status.Id == statusId.Value);
        if (dueBefore.HasValue)
            query = query.Where(x => x.DueDate != null && x.DueDate <= dueBefore.Value);

        var items = await query.ToListAsync(cancellationToken);
        // FetchMany su una collezione può duplicare le righe radice → distinct in memoria.
        return items.Distinct()
            .OrderBy(x => x.DueDate == null)        // i task con scadenza prima
            .ThenBy(x => x.DueDate)
            .ToList();
    }

    public async Task<IList<AdminTask>> FindAsync(Expression<Func<AdminTask, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AdminTask>()
            .Where(predicate)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<AdminTask> CreateAsync(AdminTask entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<AdminTask> UpdateAsync(AdminTask entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<AdminTask>()
            .FetchMany(x => x.Condominiums)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        entity.Trace(currentUser);
        entity.IsDeleted = true;
        foreach (var link in entity.Condominiums)
        {
            link.Trace(currentUser);
            link.IsDeleted = true;
        }
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<AdminTask>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<AdminTask, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AdminTask>()
            .Where(x => !x.IsDeleted)
            .CountAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AdminTask>()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<(IList<AdminTask> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<AdminTask, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<AdminTask, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<AdminTask>()
            .Where(x => !x.IsDeleted)
            .Where(filter);

        var totalCount = await query.CountAsync(cancellationToken);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var items = await query.ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
