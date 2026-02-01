using Microsoft.Extensions.Caching.Memory;

public interface ICacheManager
{
    void Set<T>(string region, string key, T value, int minutes = 5);
    T? Get<T>(string key);
    void Remove(string key);
    void Clear(string region);
}

public class CacheManager : ICacheManager
{
    private readonly IMemoryCache _cache;
    private Dictionary<string, IList<string>> regionKeys;
    public CacheManager(IMemoryCache cache)
    {
        _cache = cache;
        regionKeys = new Dictionary<string, IList<string>>();
    }

    public void Set<T>(string region, string key, T value, int minutes = 5)
    {
        if (!regionKeys.ContainsKey(region))
        {
            regionKeys.Add(region, new List<string>());
        }

        if (!regionKeys[region].Contains(key))
        {
            regionKeys[region].Add(key);
        }
        _cache.Set(key, value, TimeSpan.FromMinutes(minutes));
    }

    public T? Get<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public void Clear(string region)
    {
        if (regionKeys.ContainsKey(region))
        {
            var allKeys = regionKeys[region];
            allKeys.ToList().ForEach(key =>
            {
                _cache.Remove(key);
            });
        }
    }
}