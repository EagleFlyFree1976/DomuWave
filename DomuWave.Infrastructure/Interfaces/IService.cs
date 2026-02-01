namespace DomuWave.Services.Interfaces;

public interface IService
{
    string CacheRegion { get; }
    Task ClearCache(CancellationToken cancellationToken);
}