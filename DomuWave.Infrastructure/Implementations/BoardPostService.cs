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

public class BoardPostService : BaseService, IBoardPostService
{
    public BoardPostService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache) { }
    public override string CacheRegion => "BoardPosts";

    public async Task<IList<BoardPost>> GetByCondominiumAsync(int condominiumId, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPost>()
            .Where(x => x.Condominium.Id == condominiumId && !x.IsDeleted)
            .OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreationDate)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<BoardPost> GetByIdAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPost>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<IList<BoardPost>> GetAllAsync(IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPost>().Where(x => !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<BoardPost>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPost>().Where(x => x.Tenant.Id == tenantId && !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<BoardPost>> FindAsync(Expression<Func<BoardPost, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPost>().Where(x => !x.IsDeleted).Where(predicate).ToListAsync(ct).ConfigureAwait(false);

    public async Task<BoardPost> CreateAsync(BoardPost entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<BoardPost> UpdateAsync(BoardPost entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<BoardPost>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<BoardPost>().FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPost>().AnyAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<int> CountAsync(Expression<Func<BoardPost, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPost>().Where(x => !x.IsDeleted).Where(predicate).CountAsync(ct).ConfigureAwait(false);

    public async Task<(IList<BoardPost> Items, int TotalCount)> GetPagedAsync(Expression<Func<BoardPost, bool>> filter, int pageNumber, int pageSize, Expression<Func<BoardPost, object>> orderBy, bool ascending, IUser currentUser, CancellationToken ct)
    {
        var query = session.Query<BoardPost>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await (ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy)).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct).ConfigureAwait(false);
        return (items, total);
    }
}

public class BoardPostCommentService : BaseService, IBoardPostCommentService
{
    public BoardPostCommentService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider, cache) { }
    public override string CacheRegion => "BoardPostComments";

    public async Task<IList<BoardPostComment>> GetByPostAsync(int boardPostId, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPostComment>()
            .Where(x => x.Post.Id == boardPostId && !x.IsDeleted)
            .OrderBy(x => x.CreationDate)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<BoardPostComment> GetByIdAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPostComment>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<IList<BoardPostComment>> GetAllAsync(IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPostComment>().Where(x => !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<BoardPostComment>> GetByTenantIdAsync(Guid tenantId, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPostComment>().Where(x => x.Tenant.Id == tenantId && !x.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IList<BoardPostComment>> FindAsync(Expression<Func<BoardPostComment, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPostComment>().Where(x => !x.IsDeleted).Where(predicate).ToListAsync(ct).ConfigureAwait(false);

    public async Task<BoardPostComment> CreateAsync(BoardPostComment entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<BoardPostComment> UpdateAsync(BoardPostComment entity, IUser currentUser, CancellationToken ct)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<BoardPostComment>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);
        if (entity == null) return false;
        entity.IsDeleted = true;
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, IUser currentUser, CancellationToken ct)
    {
        var entity = await session.Query<BoardPostComment>().FirstOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        if (entity == null) return false;
        await session.DeleteAsync(entity, ct).ConfigureAwait(false);
        await session.FlushAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(int id, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPostComment>().AnyAsync(x => x.Id == id && !x.IsDeleted, ct).ConfigureAwait(false);

    public async Task<int> CountAsync(Expression<Func<BoardPostComment, bool>> predicate, IUser currentUser, CancellationToken ct)
        => await session.Query<BoardPostComment>().Where(x => !x.IsDeleted).Where(predicate).CountAsync(ct).ConfigureAwait(false);

    public async Task<(IList<BoardPostComment> Items, int TotalCount)> GetPagedAsync(Expression<Func<BoardPostComment, bool>> filter, int pageNumber, int pageSize, Expression<Func<BoardPostComment, object>> orderBy, bool ascending, IUser currentUser, CancellationToken ct)
    {
        var query = session.Query<BoardPostComment>().Where(x => !x.IsDeleted).Where(filter);
        var total = await query.CountAsync(ct).ConfigureAwait(false);
        var items = await (ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy)).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct).ConfigureAwait(false);
        return (items, total);
    }
}
