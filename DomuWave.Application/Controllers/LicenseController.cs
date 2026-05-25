using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Interfaces;
using LicenseManager.Client;
using LicenseManager.Client.Cache;
using LicenseManager.Client.Configuration;
using LicenseManager.Client.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/license")]
[Produces("application/json")]
public class LicenseController(
    ILogger<LicenseController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IOptions<LicenseManagerOptions> lmOptions,
    ILicenseManagerClient lmClient,
    ITokenCache tokenCache,
    LicenseManagerHelper licenseHelper,
    IUserService userService,
    ITenantService tenantService)
    : PrivateControllerBase(logger, configuration)
{
    private readonly LicenseManagerOptions _lmOpts = lmOptions.Value;

    /// <summary>Restituisce i piani disponibili per questa applicazione (da LicenseManager).</summary>
    [HttpGet("plans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_lmOpts.BaseUrl))
                return Ok(Array.Empty<object>());

            using var http = new HttpClient();
            http.BaseAddress = new Uri(_lmOpts.BaseUrl.TrimEnd('/'));
            http.DefaultRequestHeaders.Add("X-Api-Key", _lmOpts.ApiKey);

            var response = await http.GetAsync($"/api/apps/{_lmOpts.AppId}/plans", ct);
            if (!response.IsSuccessStatusCode)
                return Ok(Array.Empty<object>());

            var json = await response.Content.ReadAsStringAsync(ct);
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Impossibile recuperare i piani da LicenseManager.");
            return Ok(Array.Empty<object>());
        }
    }

    /// <summary>Genera l'URL handoff SSO verso LicenseManager per l'utente corrente.</summary>
    [HttpGet("handoff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHandoffUrl(CancellationToken ct)
    {
        try
        {
            // Assicura che il tenant sia registrato su LM prima del redirect
            if (TenantId.HasValue)
                await EnsureTenantRegisteredAsync(TenantId.Value, ct);

            var url = await licenseHelper.GetLicenseManagerUrlAsync(
                userId:    CurrentUser.Id.ToString(),
                email:     CurrentUser.Email    ?? string.Empty,
                firstName: CurrentUser.Name     ?? string.Empty,
                lastName:  CurrentUser.LastName ?? string.Empty,
                tenantId:  TenantId);

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Impossibile generare l'URL handoff verso LicenseManager.");
            return Ok(new { url = string.Empty });
        }
    }

    private async Task EnsureTenantRegisteredAsync(Guid tenantId, CancellationToken ct)
    {
        // Prova a caricare i token: se LM conosce già il tenant (anche senza licenze) restituisce una cache non-null
        var cache = await lmClient.FetchTokensAsync(tenantId, ct);

        // cache == null significa che FetchTokens ha avuto un'eccezione di rete → LM non raggiungibile, non fare nulla
        // cache non-null con IsFailOpen == false significa che LM ha risposto (anche con 404) → il tenant non esiste su LM
        // Distinguiamo: se la cache è stata appena scritta con IsFailOpen==false e 0 token, il tenant non è registrato
        if (cache is not null && !cache.IsFailOpen && cache.Tokens.Count == 0)
        {
            // Tenant non registrato su LM → lo registriamo ora
            var tenant = (await tenantService.GetActiveTenantsAsync(CurrentUser, ct))
                .FirstOrDefault(t => t.Id == tenantId);

            if (tenant is null) return;

            var result = await lmClient.NotifyTenantCreatedAsync(tenantId, CurrentUser.Email ?? string.Empty, tenant.Name, ct);
            if (result.Success)
                logger.LogInformation("Tenant {TenantId} ({Name}) registrato su LM al momento dell'acquisto.", tenantId, tenant.Name);
            else
                logger.LogWarning("Registrazione tenant {TenantId} su LM fallita: {Error}", tenantId, result.Error);
        }
    }

    /// <summary>
    /// Forza il refetch dei token di licenza dal LicenseManager per il tenant corrente.
    /// Utile quando il webhook di invalidazione non è stato ricevuto (es. servizio era offline).
    /// </summary>
    [HttpPost("refresh-cache")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshCache(CancellationToken ct)
    {
        if (!TenantId.HasValue)
            return BadRequest(new { error = "Tenant non disponibile." });

        tokenCache.Invalidate(TenantId.Value);
        var cache = await lmClient.FetchTokensAsync(TenantId.Value, ct);

        if (cache is null)
            return StatusCode(503, new { error = "LicenseManager non raggiungibile." });

        var features = cache.Tokens
            .Where(kv => kv.Value.IsActive)
            .Select(kv => kv.Key)
            .ToList();

        logger.LogInformation("[License] Cache aggiornata manualmente per tenant {TenantId}: {Count} feature attive.", TenantId, features.Count);

        return Ok(new { tenantId = TenantId, activeFeatures = features });
    }

    /// <summary>
    /// Notifica LicenseManager di tutti i tenant esistenti su DomuWave.
    /// Usare una tantum per sincronizzare tenant creati prima dell'integrazione LM.
    /// </summary>
    [HttpPost("sync-tenants")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncTenants(CancellationToken ct)
    {
        var tenants = await tenantService.GetActiveTenantsAsync(CurrentUser, ct);
        int ok = 0, failed = 0;

        foreach (var tenant in tenants)
        {
            try
            {
                var owner = await userService.GetByIdAsync(tenant.CreatedBy, ct);
                var ownerEmail = owner?.Email ?? string.Empty;
                var result = await lmClient.NotifyTenantCreatedAsync(tenant.Id, ownerEmail, tenant.Name, ct);
                if (result.Success) ok++; else { failed++; logger.LogWarning("Sync tenant {Id} fallita: {Err}", tenant.Id, result.Error); }
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogWarning(ex, "Errore sync tenant {Id}", tenant.Id);
            }
        }

        return Ok(new { synced = ok, failed });
    }
}
