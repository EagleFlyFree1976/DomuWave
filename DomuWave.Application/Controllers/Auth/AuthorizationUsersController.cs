using Auth.Services.Exceptions;
using Auth.Services.Extensions;
using Auth.Services.Interfaces;
using Auth.Services.Models;
using Auth.Services.Models.Dto;
using CPQ.Core.ActionFilters;
using CPQ.Core.Result;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Authorization = Auth.Services.Models.Authorization;

namespace DomuWave.Microservice.Controllers;

[Route("api/authorization")]
public class AuthorizationUsersController : AuthorizationBaseController
{
    protected readonly IAuthorizationManager AuthorizationManager;
    protected readonly IAuthUserService _userService;

    public AuthorizationUsersController(
        ILogger<AuthorizationUsersController> logger,
        IOptionsMonitor<OxCoreSettings> configuration,
        IAuthUserService userService,
        IAuthorizationManager authorizationManager)
        : base(logger, configuration)
    {
        _userService = userService;
        AuthorizationManager = authorizationManager;
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> PutUser(int id, AuthorizationRequestDTO authorizationRequestDto)
    {
        var group = await AuthorizationManager.GetUserAuthorizationById(id);
        group.CanView   = authorizationRequestDto.Can.CanView;
        group.CanModify = authorizationRequestDto.Can.CanModify;
        group.CanCreate = authorizationRequestDto.Can.CanCreate;
        group.CanDelete = authorizationRequestDto.Can.CanDelete;
        group.CanAction = authorizationRequestDto.Can.CanAction;
        await AuthorizationManager.AddAuthorizationToUser(group);
        return NoContent();
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpPost("users/authorizations")]
    public async Task<IActionResult> PostUsers([FromBody] EditAuthorization authorizationDto)
    {
        await AuthorizationManager.AddAuthorizationToUsers(authorizationDto, authorizationDto.users);
        return NoContent();
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUsers(int id)
    {
        var userAuthorizationById = await AuthorizationManager.GetUserAuthorizationById(id);
        await AuthorizationManager.RemoveUserAuthorization(userAuthorizationById);
        return NoContent();
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanAction, "Access", ModuleKey.Auth)]
    [HttpGet("users/{id:int}/can/{can:int}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(bool))]
    public async Task<IActionResult> GetCanUsers(int id, Can can, string authCode, string module = ModuleKey.Bizlio, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) throw new AuthException("Utente non trovato");
        var userIsAuthorizedFor = await AuthorizationManager.UserIsAuthorizedFor(user, authCode, can, module, cancellationToken);
        return Ok(userIsAuthorizedFor);
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanAction, "Access", ModuleKey.Auth)]
    [HttpGet("users/can")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(bool))]
    public async Task<IActionResult> GetCanI(string authCode, string module = ModuleKey.Bizlio, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetByIdAsync(CurrentUser.Id, cancellationToken);
        if (user == null) throw new AuthException("Utente non trovato");
        var canAuthorization = await AuthorizationManager.UserIsAuthorizedFor(user, authCode, module, cancellationToken);
        return Ok(canAuthorization);
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanAction, "Access", ModuleKey.Auth)]
    [HttpGet("users/{id:int}/can")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(bool))]
    public async Task<IActionResult> GetCanAllUsers(int id, string authCode, string module = ModuleKey.Bizlio)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) throw new AuthException("Utente non trovato");
        var canAuthorization = await AuthorizationManager.UserIsAuthorizedFor(user, authCode, module);
        return Ok(canAuthorization);
    }

    [HttpGet("users/{id:int}/authorizations")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, ModuleKey.Auth)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IList<AuthorizationDto>))]
    public async Task<IActionResult> GetUserAuthorizations(int id, int? idModule, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (user == null) return NotFound();
        IList<FlatUserAuthorization> allAuths = await AuthorizationManager.AllUserAuthorization(user, idModule).ConfigureAwait(false);
        return Ok(allAuths.Select(k => k.ToDto()).ToList());
    }
}
