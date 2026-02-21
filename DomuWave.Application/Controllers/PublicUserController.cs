using CPQ.Core.ActionFilters;
using CPQ.Core.Security;
using CPQ.Core.Settings;
using DocumentFormat.OpenXml.Wordprocessing;
using DomuWave.Application.Code;
using DomuWave.Application.Models;
using DomuWave.Services.Clients;
using DomuWave.Services.Clients.Request;
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
    IAuthorizationClient authorizationClient)
    : PrivateAdminControllerBase(logger, configuration)
{

    private IAuthorizationClient _authorizationClient = authorizationClient;

    [HttpPost("login")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(UserDto))]
    public async Task<IActionResult> GetByLogin([FromBody] UserLogin logininfo, CancellationToken cancellationToken)
    {


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
        



        return Ok(returnDto);
    }
}