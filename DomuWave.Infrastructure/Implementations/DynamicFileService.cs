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
}
