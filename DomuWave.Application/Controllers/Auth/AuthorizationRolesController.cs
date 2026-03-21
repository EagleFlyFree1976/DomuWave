using Auth.Services.Extensions;
using Auth.Services.Interfaces;
using Auth.Services.Models;
using Auth.Services.Models.Dto;
using CPQ.Core.ActionFilters;
using CPQ.Core.Result;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NHibernate.Linq;

namespace DomuWave.Microservice.Controllers;

[Route("api/authorization")]
public class AuthorizationRolesController : AuthorizationBaseController
{
    protected readonly IAuthorizationManager AuthorizationManager;

    public AuthorizationRolesController(
        ILogger<AuthorizationRolesController> logger,
        IOptionsMonitor<OxCoreSettings> configuration,
        IAuthorizationManager authorizationManager)
        : base(logger, configuration)
    {
        AuthorizationManager = authorizationManager;
    }

    [HttpGet]
    [Route("roles")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<BaseDto>))]
    public async Task<IActionResult> GetAll(string s, CancellationToken cancellationToken)
    {
        IQueryable<Role> queryable = AuthorizationManager.Roles();
        if (!string.IsNullOrEmpty(s))
            queryable = queryable.Where(c => c.Description.StartsWith(s));

        var roles = await queryable.ToListAsync(cancellationToken);
        var tResults = roles.Select(r => new BaseDto { Id = r.Id, Code = r.Code, Description = r.Description }).ToList();
        return new EnumerableOkObjectResult(tResults);
    }

    [HttpGet]
    [Route("roles/bymodule")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<BaseDto>))]
    public async Task<IActionResult> GetByModule(string moduleCode, CancellationToken cancellationToken)
    {
        var roles = await AuthorizationManager.GetRolesByModuleCode(moduleCode, cancellationToken);
        var tResults = roles.Select(r => new BaseDto { Id = r.Id, Code = r.Code, Description = r.Description }).ToList();
        return new EnumerableOkObjectResult(tResults);
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpGet("roles/{id:int}/authorizations")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthorizationsByRole(int id, CancellationToken cancellationToken)
    {
        var authorizations = await AuthorizationManager.GetAuthorizationsByGroupBaseId(id, cancellationToken);
        return new EnumerableOkObjectResult(authorizations.Select(a => new
        {
            id              = a.Id,
            authCode        = a.Authorization.Code,
            authDescription = a.Authorization.Description,
            canView         = a.CanView,
            canCreate       = a.CanCreate,
            canModify       = a.CanModify,
            canDelete       = a.CanDelete,
            canAction       = a.CanAction,
        }));
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpPut("roles/{id:int}")]
    public async Task<IActionResult> PutRole(int id, AuthorizationRequestDTO authorizationRequestDto, CancellationToken cancellationToken)
    {
        var group = await AuthorizationManager.GetGroupAuthorizationById(id);
        group.CanView   = authorizationRequestDto.Can.CanView;
        group.CanModify = authorizationRequestDto.Can.CanModify;
        group.CanCreate = authorizationRequestDto.Can.CanCreate;
        group.CanDelete = authorizationRequestDto.Can.CanDelete;
        group.CanAction = authorizationRequestDto.Can.CanAction;
        await AuthorizationManager.SaveGroupAuthorization(group, cancellationToken);
        return NoContent();
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpPost("role")]
    public async Task<IActionResult> PostRole(AuthorizationRequestDTO authorizationRequestDto, CancellationToken cancellationToken)
    {
        var authorization = await AuthorizationManager.GetAuthorizationByCode(authorizationRequestDto.AuthCode);
        var groupCreated  = await AuthorizationManager.GetGroupBaseById(authorizationRequestDto.EntityId);
        var saved         = await AuthorizationManager.AddAuthorizationToGroup(groupCreated, authorization, cancellationToken);

        var group = await AuthorizationManager.GetGroupAuthorizationById(saved.Id);
        group.CanView   = authorizationRequestDto.Can.CanView;
        group.CanModify = authorizationRequestDto.Can.CanModify;
        group.CanCreate = authorizationRequestDto.Can.CanCreate;
        group.CanDelete = authorizationRequestDto.Can.CanDelete;
        group.CanAction = authorizationRequestDto.Can.CanAction;
        await AuthorizationManager.SaveGroupAuthorization(group, cancellationToken);

        return new PostOkResult(saved.Id.ToString());
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpPost("roles")]
    public async Task<IActionResult> PostRoles(AuthorizationGroupsRequestDto authorizationGroupsRequestDto, CancellationToken cancellationToken)
    {
        await AuthorizationManager.AddAuthorizationToGroups(authorizationGroupsRequestDto, cancellationToken);
        return NoContent();
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpDelete("roles/{id:int}")]
    public async Task<IActionResult> DeleteGroups(int id, CancellationToken cancellationToken)
    {
        await AuthorizationManager.RemoveAuthorizationFromGroup(id, cancellationToken);
        return NoContent();
    }
}
