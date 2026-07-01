using Auth.Services.Interfaces;
using Auth.Services.Models;
using CPQ.Core.ActionFilters;
using CPQ.Core.Security;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Application.Models;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NHibernate.Linq;
using UserLogin = DomuWave.Services.Clients.Request.UserLogin;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione tenant (organizzazioni/studi di amministrazione)
/// </summary>
[Route("api/[controller]")]
[NoAccessTokenRequired]
public class PublicUserController(
    ILogger<PublicUserController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IAuthUserService authUserService,
    IUserTenantService userTenantService,
    IUserService userService)
    : PrivateAdminControllerBase(logger, configuration)
{

    private IAuthUserService _authUserService = authUserService;
    private IUserTenantService _userTenantService = userTenantService;
    private IUserService _userService = userService;

    [HttpPost("request-reset-password")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestResetPassword(
        [FromBody] RequestResetPasswordDto request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email obbligatoria" });

        // reset-password via Auth.Services is handled by api/auth/reset-password
        return Ok(new { message = "Email inviata" });
    }

    [HttpPost("login")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(UserDto))]
    public async Task<IActionResult> GetByLogin([FromBody] UserLogin logininfo, CancellationToken cancellationToken)
    {
        var systemUser = await _userService.GetByTokenAsync(CommonKeys.SystemUserToken, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            User authUser = await _authUserService.GetQueryable()
                .Where(u => u.Name == logininfo.Email && !u.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (authUser == null || authUser.Password != logininfo.Password.EncryptString())
                return NotFound();

            // Auto-riparazione: alcuni utenti storici hanno Token nullo. Se manca,
            // lo si genera e persiste al primo login (formula = Id cifrato).
            var token = string.IsNullOrEmpty(authUser.Token)
                ? await _authUserService.EnsureUserTokenAsync(authUser.Id, cancellationToken).ConfigureAwait(false)
                : authUser.Token;

            UserDto returnDto = new UserDto
            {
                FullName = authUser.FullName,
                Id       = authUser.Id,
                LastName = authUser.LastName,
                Name     = authUser.Name,
                Token    = token,
                Role     = authUser.Role?.Code,
                IsActive = authUser.IsActive,
            };

            CPQ.Core.Memberships.User? _user = await _userService.GetByIdAsync(authUser.Id, cancellationToken).ConfigureAwait(false);

            // Determina se l'utente è condòmino in ALMENO un tenant (RoleCode per-tenant).
            var allUserTenants = await _userTenantService
                .GetByUserIdAsync(authUser.Id, systemUser, cancellationToken)
                .ConfigureAwait(false);
            var activeLinks = allUserTenants.Where(ut => ut.IsActive && !ut.IsDeleted).ToList();
            var hasCondominoTenant = activeLinks.Any(ut =>
                string.Equals(ut.RoleCode, "Condomino", StringComparison.OrdinalIgnoreCase));
            var hasAdminTenant = activeLinks.Any(ut =>
                !string.Equals(ut.RoleCode, "Condomino", StringComparison.OrdinalIgnoreCase));

            // Tenant di default (per scegliere il profilo iniziale).
            var defaultLink = activeLinks.FirstOrDefault(ut => ut.IsDefault) ?? activeLinks.FirstOrDefault();
            var defaultIsCondomino = defaultLink != null &&
                string.Equals(defaultLink.RoleCode, "Condomino", StringComparison.OrdinalIgnoreCase);

            if (_user.IsSystemUser)
            {
                returnDto.Profile = UserProfile.SuperAdmin;
            }
            else if (defaultLink != null)
            {
                // Profilo iniziale = ruolo nel tenant di default (per-tenant).
                returnDto.Profile = defaultIsCondomino ? UserProfile.User : UserProfile.TenantAdministrator;
            }
            else
            {
                // Fallback legacy: nessun UserTenant → ruolo globale.
                returnDto.Profile = _user.Role?.Code?.ToLower() == "condomino"
                    ? UserProfile.User
                    : UserProfile.TenantAdministrator;
            }

            // Lo STESSO utente può avere più ruoli su tenant diversi (es. admin nel
            // proprio tenant + condòmino nel tenant di un altro amministratore).
            // Popoliamo SEMPRE entrambe le liste: il selettore in sidebar le unisce
            // e il profilo viene ricalcolato a ogni cambio (profile-for-tenant).

            // 1. Condomìni dove l'utente è condòmino (qualsiasi tenant).
            var condominiums = await _userTenantService
                .GetCondominiumsByCondominoUserIdAsync(authUser.Id, systemUser, cancellationToken)
                .ConfigureAwait(false);
            returnDto.AvailableCondominiums = condominiums;

            // 2. Tenant dove l'utente è amministratore (RoleCode != Condomino).
            var adminTenants = activeLinks
                .Where(ut => ut.Tenant.IsActive
                    && !string.Equals(ut.RoleCode, "Condomino", StringComparison.OrdinalIgnoreCase))
                .Select(k => k.ToDto())
                .ToList();
            returnDto.AvailableTenants = adminTenants
                .Select(k => new TenantReadDto { Code = k.TenantCode, Name = k.TenantName, Id = k.TenantId, IsPrimary = k.IsDefault })
                .ToList();

            // 3. Tenant/condominio iniziale, in base al profilo di partenza.
            if (returnDto.Profile == UserProfile.User)
            {
                var initial = (defaultLink != null && defaultIsCondomino
                                  ? condominiums.FirstOrDefault(c => c.TenantId == defaultLink.Tenant.Id)
                                  : null)
                              ?? condominiums.FirstOrDefault();
                if (initial != null)
                    returnDto.Tenant = new TenantReadDto { Id = initial.TenantId, Name = initial.CondominiumName };
            }
            else
            {
                var defaultTenant = adminTenants.FirstOrDefault(j => j.IsDefault) ?? adminTenants.FirstOrDefault();
                if (defaultTenant != null)
                    returnDto.Tenant = new TenantReadDto
                    {
                        Code = defaultTenant.TenantCode, Name = defaultTenant.TenantName,
                        Id = defaultTenant.TenantId, IsPrimary = defaultTenant.IsDefault
                    };
            }

            return Ok(returnDto);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}

public record RequestResetPasswordDto(string Email);