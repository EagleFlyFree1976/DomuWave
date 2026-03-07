using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class ExtraordinaryWorkService : BaseService, IExtraordinaryWorkService
{
    public ExtraordinaryWorkService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "ExtraordinaryWork";

    public async Task<ExtraordinaryWork> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ExtraordinaryWork>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<IList<ExtraordinaryWork>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ExtraordinaryWork>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<IList<ExtraordinaryWork>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ExtraordinaryWork>()
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<IList<ExtraordinaryWork>> FindAsync(Expression<Func<ExtraordinaryWork, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ExtraordinaryWork>()
            .Where(predicate).Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<ExtraordinaryWork> CreateAsync(ExtraordinaryWork entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<ExtraordinaryWork> UpdateAsync(ExtraordinaryWork entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<ExtraordinaryWork>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        entity.Trace(currentUser);
        entity.IsDeleted = true;
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<ExtraordinaryWork>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<ExtraordinaryWork, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ExtraordinaryWork>().Where(x => !x.IsDeleted).CountAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ExtraordinaryWork>().AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task<(IList<ExtraordinaryWork> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<ExtraordinaryWork, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<ExtraordinaryWork, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<ExtraordinaryWork>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(cancellationToken);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return (await query.ToListAsync(cancellationToken), total);
    }

    public async Task<IList<ExtraordinaryWork>> GetByCondominiumIdAsync(int condominiumId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ExtraordinaryWork>()
            .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
            .OrderByDescending(x => x.RequestedDate)
            .ToListAsync(cancellationToken);

    public async Task<IList<ExtraordinaryWork>> GetByStatusAsync(int condominiumId, string status, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<ExtraordinaryWork>()
            .Where(x => x.Condominium.Id == condominiumId && x.Status == status && !x.IsDeleted)
            .OrderByDescending(x => x.RequestedDate)
            .ToListAsync(cancellationToken);
}
