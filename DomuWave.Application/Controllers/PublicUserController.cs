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

            UserDto returnDto = new UserDto
            {
                FullName = authUser.FullName,
                Id       = authUser.Id,
                LastName = authUser.LastName,
                Name     = authUser.Name,
                Token    = authUser.Token,
                Role     = authUser.Role?.Code,
                IsActive = authUser.IsActive,
            };

            var _user = await _userService.GetByIdAsync(authUser.Id, cancellationToken).ConfigureAwait(false);
            if (_user.IsSystemUser)
            {
                returnDto.Profile = UserProfile.SuperAdmin;
            }
            else
            {
                returnDto.Profile = _user.Role?.Code?.ToLower() == "condomino"
                    ? UserProfile.User
                    : UserProfile.TenantAdministrator;
            }

            if (returnDto.Profile == UserProfile.User)
            {
                var condominiums = await _userTenantService
                    .GetCondominiumsByCondominoUserIdAsync(authUser.Id, systemUser, cancellationToken)
                    .ConfigureAwait(false);

                returnDto.AvailableCondominiums = condominiums;

                var first = condominiums.FirstOrDefault();
                if (first != null)
                    returnDto.Tenant = new TenantReadDto { Id = first.TenantId, Name = first.CondominiumName };
            }
            else
            {
                IList<UserTenant> tenants = await _userTenantService
                    .GetByUserIdAsync(authUser.Id, systemUser, cancellationToken)
                    .ConfigureAwait(false);

                var tenantsDto = tenants.Where(k => k.Tenant.IsActive && k.IsActive).Select(j => j.ToDto()).ToList();
                var defaultTenant = tenantsDto.FirstOrDefault(j => j.IsDefault) ?? tenantsDto.FirstOrDefault();

                if (tenants.Any() && defaultTenant != null)
                {
                    returnDto.AvailableTenants = tenantsDto
                        .Select(k => new TenantReadDto { Code = k.TenantCode, Name = k.TenantName, Id = k.TenantId, IsPrimary = k.IsDefault })
                        .ToList();
                    returnDto.Tenant = new TenantReadDto
                    {
                        Code = defaultTenant.TenantCode, Name = defaultTenant.TenantName,
                        Id = defaultTenant.TenantId, IsPrimary = defaultTenant.IsDefault
                    };
                }
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