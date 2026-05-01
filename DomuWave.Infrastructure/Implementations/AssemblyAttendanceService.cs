using System.Linq.Expressions;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class AssemblyAttendanceService : BaseService, IAssemblyAttendanceService
{
    public AssemblyAttendanceService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "AssemblyAttendances";

    public async Task<AssemblyAttendance?> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAttendance>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<AssemblyAttendance>> GetAllAsync(IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAttendance>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<AssemblyAttendance>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAttendance>()
            .Where(x => x.Tenant.Id == tenantId && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<AssemblyAttendance>> GetByAssemblyIdAsync(int assemblyId, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAttendance>()
            .Where(x => x.Assembly.Id == assemblyId && !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IList<AssemblyAttendance>> FindAsync(Expression<Func<AssemblyAttendance, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAttendance>()
            .Where(predicate)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<AssemblyAttendance> CreateAsync(AssemblyAttendance entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<AssemblyAttendance> UpdateAsync(AssemblyAttendance entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var entity = await session.Query<AssemblyAttendance>()
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
        var entity = await session.Query<AssemblyAttendance>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await session.FlushAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<int> CountAsync(Expression<Func<AssemblyAttendance, bool>> predicate, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAttendance>()
            .Where(x => !x.IsDeleted)
            .CountAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken cancellationToken)
        => await session.Query<AssemblyAttendance>()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

    public async Task<(IList<AssemblyAttendance> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<AssemblyAttendance, bool>> filter, int pageNumber, int pageSize,
        Expression<Func<AssemblyAttendance, object>> orderBy, bool ascending,
        IUser currentUser, CancellationToken cancellationToken)
    {
        var query = session.Query<AssemblyAttendance>()
            .Where(x => !x.IsDeleted)
            .Where(filter);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return (items, totalCount);
    }
}
