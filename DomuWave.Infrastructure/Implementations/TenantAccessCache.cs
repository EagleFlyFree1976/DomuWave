using System.Collections.Concurrent;
using DomuWave.Services.Interfaces;

namespace DomuWave.Services.Implementations;

public class TenantAccessCache : ITenantAccessCache
{
    // key: "userId:tenantId"  value: hasAccess
    private readonly ConcurrentDictionary<string, bool> _store = new();

    private static string Key(int userId, Guid tenantId) => $"{userId}:{tenantId}";

    public bool TryGetAccess(int userId, Guid tenantId, out bool hasAccess)
        => _store.TryGetValue(Key(userId, tenantId), out hasAccess);

    public void SetAccess(int userId, Guid tenantId, bool hasAccess)
        => _store[Key(userId, tenantId)] = hasAccess;

    public void InvalidateForTenant(Guid tenantId)
    {
        var suffix = $":{tenantId}";
        foreach (var key in _store.Keys.Where(k => k.EndsWith(suffix, StringComparison.Ordinal)).ToList())
            _store.TryRemove(key, out _);
    }

    public void InvalidateEntry(int userId, Guid tenantId)
        => _store.TryRemove(Key(userId, tenantId), out _);
}
