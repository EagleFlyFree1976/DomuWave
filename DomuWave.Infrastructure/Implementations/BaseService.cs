using DomuWave.Services.Interfaces;
using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;

namespace DomuWave.Services.Implementations;

public abstract class BaseService : ServiceBase, IService
{
    protected readonly ICacheManager _cache;
    protected BaseService(ISessionFactoryProvider sessionFactoryProvider, ICacheManager cache) : base(sessionFactoryProvider)
    {
        _cache = cache;
    }

    public abstract string CacheRegion { get; }

    public async Task ClearCache(CancellationToken cancellationToken)
    {
        _cache.Clear(CacheRegion);
    }
}