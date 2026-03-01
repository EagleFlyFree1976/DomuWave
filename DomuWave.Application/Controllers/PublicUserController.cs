using CPQ.Core.ActionFilters;
using CPQ.Core.Security;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DocumentFormat.OpenXml.Wordprocessing;
using DomuWave.Application.Code;
using DomuWave.Application.Models;
using DomuWave.Services.Clients;
using DomuWave.Services.Clients.Request;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione tenant (organizzazioni/studi di amministrazione)
/// </summary>
[Route("api/[controller]")]
[NoAccessTokenRequired]
public class PublicUserController(
    ILogger<PublicUserController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IAuthorizationClient authorizationClient,
    IUserTenantService userTenantService,
    IUserService userService)
    : PrivateAdminControllerBase(logger, configuration)
{

    private IAuthorizationClient _authorizationClient = authorizationClient;
    private IUserTenantService _userTenantService = userTenantService;
    private IUserService _userService = userService;
    [HttpPost("login")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(UserDto))]
    public async Task<IActionResult> GetByLogin([FromBody] UserLogin logininfo, CancellationToken cancellationToken)
    {

        var systemUser = await _userService.GetByTokenAsync(CommonKeys.SystemUserToken, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            var user = await _authorizationClient.Login(CommonKeys.SystemUserToken, logininfo, cancellationToken)
                .ConfigureAwait(false);
            if (user == null)
            {
                return NotFound();
            }

            UserDto returnDto = new UserDto();
            returnDto.FullName = user.FullName;
            returnDto.Id = user.Id;
            returnDto.LastName = user.LastName;
            returnDto.Name = user.Name;
            returnDto.Token = user.Token;
            returnDto.Role = user.Role;
            returnDto.Path = user.Path;
            returnDto.IsActive = user.IsActive;

            var _user = await _userService.GetByIdAsync(user.Id, cancellationToken).ConfigureAwait(false);
            if (_user.IsSystemUser)
            {
                returnDto.Profile = UserProfile.SuperAdmin;
            }
            else
            {
                if (_user.Role.Code.ToLower() == "condomino")
                {
                    returnDto.Profile = UserProfile.User;
                }
                else
                {
                    returnDto.Profile = UserProfile.TenantAdministrator;
                }
            }

          
            if (returnDto.Profile == UserProfile.User)
            {
                var condominiums = await _userTenantService.GetCondominiumsByCondominoUserIdAsync(user.Id, systemUser, cancellationToken)
                    .ConfigureAwait(false);

                returnDto.AvailableCondominiums = condominiums;

                var first = condominiums.FirstOrDefault();
                if (first != null)
                {
                    returnDto.Tenant = new TenantReadDto { Id = first.TenantId, Name = first.CondominiumName };
                }
            }

            else
            {


                IList<UserTenant> tenants = await _userTenantService.GetByUserIdAsync(user.Id, systemUser, cancellationToken)
                    .ConfigureAwait(false);
                    

        var tenantsDto = tenants.ToList().Where(k=>k.Tenant.IsActive && k.IsActive).Select(j => j.ToDto());
            var defaultTenant = tenantsDto.FirstOrDefault(j => j.IsDefault);
            if (defaultTenant == null)
            {
                defaultTenant = tenantsDto.FirstOrDefault();
            }
            if (tenants.Any() && defaultTenant != null)
            {
                returnDto.AvailableTenants = tenantsDto.Select(k => new TenantReadDto() { Code = k.TenantCode, Name = k.TenantName, Id = k.TenantId, IsPrimary = k.IsDefault }).ToList();
                returnDto.Tenant = new TenantReadDto()
                {
                    Code = defaultTenant.TenantCode, Name = defaultTenant.TenantName, Id = defaultTenant.TenantId,
                    IsPrimary = defaultTenant.IsDefault
                };
            }


            }

            return Ok(returnDto);
        }
        catch (Exception exc)
        {
            return NotFound();
        }
    }
}