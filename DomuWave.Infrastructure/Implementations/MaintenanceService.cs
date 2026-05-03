using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class MaintenanceService : BaseService, IMaintenanceService
{
    public MaintenanceService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "Maintenance";

    public async Task<Maintenance> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IList<Maintenance>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<IList<Maintenance>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<IList<Maintenance>> FindAsync(Expression<Func<Maintenance, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .Where(predicate)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<Maintenance> CreateAsync(Maintenance entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<Maintenance> UpdateAsync(Maintenance entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<Maintenance>()
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
        var entity = await session.Query<Maintenance>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<Maintenance, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .Where(x => !x.IsDeleted)
            .CountAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<(IList<Maintenance> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<Maintenance, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<Maintenance, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<Maintenance>()
            .Where(x => !x.IsDeleted)
            .Where(filter);

        var totalCount = await query.CountAsync(cancellationToken);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        var items = await query.ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<IList<Maintenance>> GetByCondominiumIdAsync(int condominiumId, Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .Where(x => x.Condominium.Id == condominiumId && x.Tenant.Id == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.ReportedDate)
            .ToListAsync(cancellationToken);

    public async Task<IList<Maintenance>> GetByStatusAsync(int condominiumId, string status, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .Where(x => x.Condominium.Id == condominiumId && x.Status == status && !x.IsDeleted)
            .OrderByDescending(x => x.ReportedDate)
            .ToListAsync(cancellationToken);

    public async Task<IList<Maintenance>> GetByPriorityAsync(int condominiumId, string priority, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .Where(x => x.Condominium.Id == condominiumId && x.Priority == priority && !x.IsDeleted)
            .OrderByDescending(x => x.ReportedDate)
            .ToListAsync(cancellationToken);

    public async Task<IList<Maintenance>> GetOpenAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<Maintenance>()
            .Where(x => x.Condominium.Id == condominiumId && x.Status != "Completed" && x.Status != "Cancelled" && !x.IsDeleted)
            .OrderByDescending(x => x.ReportedDate)
            .ToListAsync(cancellationToken);
}
