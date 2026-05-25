namespace DomuWave.Services.Interfaces;

public interface ITenantAccessCache
{
    bool TryGetAccess(int userId, Guid tenantId, out bool hasAccess);
    void SetAccess(int userId, Guid tenantId, bool hasAccess);

    /// <summary>
    /// Rimuove tutte le entry relative a un tenant (es. tenant disabilitato/riabilitato/eliminato).
    /// </summary>
    void InvalidateForTenant(Guid tenantId);

    /// <summary>
    /// Rimuove la singola entry per un utente+tenant (es. UserTenant creato/aggiornato/eliminato).
    /// </summary>
    void InvalidateEntry(int userId, Guid tenantId);
}
