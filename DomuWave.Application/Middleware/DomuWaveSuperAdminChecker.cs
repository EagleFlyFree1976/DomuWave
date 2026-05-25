using CPQ.Core.Memberships;
using CPQ.Core.Services;
using LicenseManager.Client.TenantResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DomuWave.Application.Middleware;

/// <summary>
/// Implementazione DomuWave di ISuperAdminChecker.
/// Carica l'utente corrente tramite X-Auth-Token header e verifica IsSystemUser.
/// </summary>
public class DomuWaveSuperAdminChecker(
    IUserService                          userService,
    ILogger<DomuWaveSuperAdminChecker>    logger) : ISuperAdminChecker
{
    public async Task<bool> IsSuperAdminAsync(HttpContext context, CancellationToken ct = default)
    {
        // Prova prima context.User (se CPQ.Core l'ha già popolato upstream)
        if (context.User is IUser ctxUser)
        {
            logger.LogDebug(
                "[SuperAdminChecker] context.User is IUser — IsSystemUser={IsSystemUser}",
                ctxUser.IsSystemUser);
            return ctxUser.IsSystemUser;
        }

        // Fallback: leggi X-Auth-Token e carica l'utente tramite CPQ.Core
        var token = context.Request.Headers["X-Auth-Token"].FirstOrDefault();
        if (string.IsNullOrEmpty(token))
        {
            logger.LogDebug("[SuperAdminChecker] Nessun X-Auth-Token nell'header — non SuperAdmin.");
            return false;
        }

        logger.LogDebug("[SuperAdminChecker] Caricamento utente tramite X-Auth-Token...");
        var user = await userService.GetByTokenAsync(token, ct).ConfigureAwait(false);

        if (user is null)
        {
            logger.LogWarning("[SuperAdminChecker] Utente non trovato per il token fornito.");
            return false;
        }

        var isSuperAdmin = user is User u && u.IsSystemUser;
        logger.LogDebug(
            "[SuperAdminChecker] Utente trovato: Id={UserId}, IsSystemUser={IsSystemUser}",
            user.Id, isSuperAdmin);

        return isSuperAdmin;
    }
}
