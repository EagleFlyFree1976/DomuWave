using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

public class DynamicFileService : BaseService, IDynamicFileService
{
    public DynamicFileService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache)
        : base(sessionFactoryProvider, cache) { }

    public override string CacheRegion => "DynamicFiles";

    public async Task<IList<DynamicFile>> GetByEntityAsync(
        string entityName, int entityId, IUser currentUser, CancellationToken ct)
        => await session.Query<DynamicFile>()
            .Where(x => x.EntityName == entityName && x.EntityId == entityId && !x.IsDeleted)
            .OrderByDescending(x => x.CreationDate)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<IList<DynamicFile>> GetByEntityFullNameAsync(
        string entityFullName, int entityId, IUser currentUser, CancellationToken ct)
        => await session.Query<DynamicFile>()
            .Where(x => x.EntityFullName == entityFullName && x.EntityId == entityId && !x.IsDeleted)
            .OrderByDescending(x => x.CreationDate)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<DynamicFileContent?> GetContentAsync(int fileId, IUser currentUser, CancellationToken ct)
        => await session.Query<DynamicFileContent>()
            .FirstOrDefaultAsync(x => x.File.Id == fileId && !x.IsDeleted, ct)
            .ConfigureAwait(false);

    public async Task<DynamicFile> UpdateAsync(DynamicFile entity, IUser currentUser, CancellationToken cancellationToken)
    {
        entity.Trace(currentUser);
        await session.SaveOrUpdateAsync(entity, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return entity;
    }

    public async Task<DynamicFile> GetByIdAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        return await session.Query<DynamicFile>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, IUser currentUser, CancellationToken cancellationToken)
    {
        var document = await session.Query<DynamicFile>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (document == null)
            return false;

        document.Trace(currentUser);
        document.IsDeleted = true;
        await session.SaveOrUpdateAsync(document, cancellationToken);
        await session.FlushAsync(cancellationToken);
        return true;
    }
}
