using DomuWave.Services.Models;

namespace DomuWave.Services.Interfaces;

/// <summary>
/// Seeds the standard Italian chart-of-accounts template for a tenant.
/// Called on tenant creation and available for backfill.
/// </summary>
public interface IChartOfAccountsTemplateSeedService
{
    /// <summary>
    /// Creates the default "Piano dei Conti Standard Italiano" template for the given tenant
    /// if it does not already exist (idempotent).
    /// </summary>
    Task SeedAsync(Tenant tenant, CancellationToken cancellationToken);
}
