using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class FaultService : BaseService, IFaultService
{
    public FaultService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache) { }
    public override string CacheRegion => "Faults";

    public async Task<IList<Fault>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken ct)
        => await session.Query<Fault>()
            .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
            .OrderByDescending(x => x.CreationDate)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<Fault> GetByIdAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<Fault>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<IList<Fault>> GetAllAsync(IUser currentUser, CancellationToken ct)
        => await session.Query<Fault>().Where(x => !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<Fault>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken ct)
        => await session.Query<Fault>().Where(x => x.Tenant.Id == tenantId && !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<Fault>> FindAsync(Expression<Func<Fault, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<Fault>().Where(x => !x.IsDeleted).Where(predicate).ToListAsync(ct).ConfigureAwait(false);

    public async Task<Fault> CreateAsync(Fault entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<Fault> UpdateAsync(Fault entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<Fault>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<Fault>().FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<Fault>().AnyAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<int> CountAsync(Expression<Func<Fault, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<Fault>().Where(x => !x.IsDeleted).Where(predicate).CountAsync(ct).ConfigureAwait(false);

    public async Task<(IList<Fault> Items, int TotalCount)> GetPagedAsync(Expression<Func<Fault, bool>> filter, int pageNumber, int pageSize, Expression<Func<Fault, object>> orderBy, bool ascending, IUser currentUser, CancellationToken ct)
    {
        var query = session.Query<Fault>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await (ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy)).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct).ConfigureAwait(false);
        return (items, total);
    }
}

public class FaultMessageService : BaseService, IFaultMessageService
{
    public FaultMessageService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache) { }
    public override string CacheRegion => "FaultMessages";

    public async Task<IList<FaultMessage>> GetByFaultAsync(int faultId, IUser currentUser, CancellationToken ct)
        => await session.Query<FaultMessage>()
            .Where(x => x.Fault.Id == faultId && !x.IsDeleted)
            .OrderBy(x => x.CreationDate)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<FaultMessage> GetByIdAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<FaultMessage>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<IList<FaultMessage>> GetAllAsync(IUser currentUser, CancellationToken ct)
        => await session.Query<FaultMessage>().Where(x => !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<FaultMessage>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken ct)
        => await session.Query<FaultMessage>().Where(x => x.Tenant.Id == tenantId && !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<FaultMessage>> FindAsync(Expression<Func<FaultMessage, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<FaultMessage>().Where(x => !x.IsDeleted).Where(predicate).ToListAsync(ct).ConfigureAwait(false);

    public async Task<FaultMessage> CreateAsync(FaultMessage entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<FaultMessage> UpdateAsync(FaultMessage entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<FaultMessage>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<FaultMessage>().FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<FaultMessage>().AnyAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<int> CountAsync(Expression<Func<FaultMessage, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<FaultMessage>().Where(x => !x.IsDeleted).Where(predicate).CountAsync(ct).ConfigureAwait(false);

    public async Task<(IList<FaultMessage> Items, int TotalCount)> GetPagedAsync(Expression<Func<FaultMessage, bool>> filter, int pageNumber, int pageSize, Expression<Func<FaultMessage, object>> orderBy, bool ascending, IUser currentUser, CancellationToken ct)
    {
        var query = session.Query<FaultMessage>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await (ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy)).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct).ConfigureAwait(false);
        return (items, total);
    }
}
