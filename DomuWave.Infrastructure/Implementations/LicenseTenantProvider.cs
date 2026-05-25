using CPQ.Core.Persistence.SessionFactories;
using CPQ.Core.Services;
using DomuWave.Services.Models;
using LicenseManager.Client.TenantResolver;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;

namespace DomuWave.Services.Implementations;

/// <summary>
/// Fornisce al SDK LicenseManager la lista di tutti i tenant attivi,
/// usata al boot per il warm-up della cache token.
/// </summary>
public class LicenseTenantProvider(ISessionFactoryProvider sessionFactoryProvider)
    : ServiceBase(sessionFactoryProvider), ITenantProvider
{
    public async Task<IEnumerable<Guid>> GetAllTenantIdsAsync(CancellationToken ct = default)
        => await session.Query<Tenant>()
            .Where(t => !t.IsDeleted && t.IsEnabled)
            .Select(t => t.Id)
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
