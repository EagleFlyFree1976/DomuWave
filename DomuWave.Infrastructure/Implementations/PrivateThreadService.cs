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

public class PrivateThreadService : BaseService, IPrivateThreadService
{
    public PrivateThreadService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache) { }
    public override string CacheRegion => "PrivateThreads";

    public async Task<IList<PrivateThread>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateThread>()
            .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
            .OrderByDescending(x => x.LastUpdateDate)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<PrivateThread?> GetByCondominiumAndUserAsync(int condominiumId, long condominioUserId, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateThread>()
            .FirstOrDefaultAsync(x => x.Condominium.Id == condominiumId && x.CondominioUserId == condominioUserId && !x.IsDeleted, ct)
            .ConfigureAwait(false);

    public async Task<PrivateThread> GetByIdAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateThread>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<IList<PrivateThread>> GetAllAsync(IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateThread>().Where(x => !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<PrivateThread>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateThread>().Where(x => x.Tenant.Id == tenantId && !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<PrivateThread>> FindAsync(Expression<Func<PrivateThread, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateThread>().Where(x => !x.IsDeleted).Where(predicate).ToListAsync(ct).ConfigureAwait(false);

    public async Task<PrivateThread> CreateAsync(PrivateThread entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<PrivateThread> UpdateAsync(PrivateThread entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<PrivateThread>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<PrivateThread>().FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateThread>().AnyAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<int> CountAsync(Expression<Func<PrivateThread, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateThread>().Where(x => !x.IsDeleted).Where(predicate).CountAsync(ct).ConfigureAwait(false);

    public async Task<(IList<PrivateThread> Items, int TotalCount)> GetPagedAsync(Expression<Func<PrivateThread, bool>> filter, int pageNumber, int pageSize, Expression<Func<PrivateThread, object>> orderBy, bool ascending, IUser currentUser, CancellationToken ct)
    {
        var query = session.Query<PrivateThread>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await (ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy)).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct).ConfigureAwait(false);
        return (items, total);
    }
}

public class PrivateMessageService : BaseService, IPrivateMessageService
{
    public PrivateMessageService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache) { }
    public override string CacheRegion => "PrivateMessages";

    public async Task<IList<PrivateMessage>> GetByThreadAsync(int threadId, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateMessage>()
            .Where(x => x.Thread.Id == threadId && !x.IsDeleted)
            .OrderBy(x => x.CreationDate)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<PrivateMessage> GetByIdAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateMessage>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<IList<PrivateMessage>> GetAllAsync(IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateMessage>().Where(x => !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<PrivateMessage>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateMessage>().Where(x => x.Tenant.Id == tenantId && !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<PrivateMessage>> FindAsync(Expression<Func<PrivateMessage, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateMessage>().Where(x => !x.IsDeleted).Where(predicate).ToListAsync(ct).ConfigureAwait(false);

    public async Task<PrivateMessage> CreateAsync(PrivateMessage entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<PrivateMessage> UpdateAsync(PrivateMessage entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<PrivateMessage>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<PrivateMessage>().FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateMessage>().AnyAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<int> CountAsync(Expression<Func<PrivateMessage, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<PrivateMessage>().Where(x => !x.IsDeleted).Where(predicate).CountAsync(ct).ConfigureAwait(false);

    public async Task<(IList<PrivateMessage> Items, int TotalCount)> GetPagedAsync(Expression<Func<PrivateMessage, bool>> filter, int pageNumber, int pageSize, Expression<Func<PrivateMessage, object>> orderBy, bool ascending, IUser currentUser, CancellationToken ct)
    {
        var query = session.Query<PrivateMessage>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await (ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy)).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct).ConfigureAwait(false);
        return (items, total);
    }
}
