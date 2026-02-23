using CPQ.Core.ActionFilters;
using CPQ.Core.Security;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DocumentFormat.OpenXml.Wordprocessing;
using DomuWave.Application.Code;
using DomuWave.Application.Models;
using DomuWave.Services.Clients;
using DomuWave.Services.Clients.Request;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
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
        var user = await _authorizationClient.Login(CommonKeys.SystemUserToken, logininfo, cancellationToken).ConfigureAwait(false);

        if (user == null)
            return NotFound();


        UserDto returnDto = new UserDto();
        returnDto.FullName = user.FullName;
        returnDto.Id = user.Id;
        returnDto.LastName = user.LastName;
        returnDto.Name = user.Name;
        returnDto.Token = user.Token;
        returnDto.Role = user.Role;
        returnDto.Path= user.Path;

        var tenants = await _userTenantService.GetByUserIdAsync(user.Id, systemUser, cancellationToken).ConfigureAwait(false);

        var tenantsDto = tenants.ToList().Select(j => j.ToDto());
        
        returnDto.AvailableTenants = tenantsDto.ToList();
        returnDto.Tenant = tenantsDto.FirstOrDefault(j => j.IsPrimary);
        return Ok(returnDto);
    }
}