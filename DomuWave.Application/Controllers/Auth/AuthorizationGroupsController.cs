using Auth.Services.Extensions;
using Auth.Services.Interfaces;
using Auth.Services.Models;
using Auth.Services.Models.Dto;
using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Result;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NHibernate.Linq;

namespace DomuWave.Microservice.Controllers;

[Route("api/authorization")]
public class AuthorizationGroupsController : AuthorizationBaseController
{
    protected readonly IAuthorizationManager AuthorizationManager;

    public AuthorizationGroupsController(
        ILogger<AuthorizationGroupsController> logger,
        IOptionsMonitor<OxCoreSettings> configuration,
        IAuthorizationManager authorizationManager)
        : base(logger, configuration)
    {
        AuthorizationManager = authorizationManager;
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpGet("authorizations")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAuthorizations(CancellationToken cancellationToken)
    {
        var authorizations = await AuthorizationManager.FindAllAuthorizations();
        return new EnumerableOkObjectResult(authorizations.Select(a => new
        {
            id              = a.Id,
            code            = a.Code,
            description     = a.Description,
            areaCode        = a.Area?.Code,
            areaDescription = a.Area?.Description,
            moduleCode      = a.Area?.Module?.Code,
        }));
    }

    [HttpPut]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [Route("groups/{id:int}")]
    public async Task<IActionResult> PutGroup(int id, AuthorizationRequestDTO authorizationRequestDto, CancellationToken cancellationToken)
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

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [Route("group")]
    public async Task<IActionResult> PostGroup(AuthorizationRequestDTO authorizationRequestDto, CancellationToken cancellationToken)
    {
        var authorization = await AuthorizationManager.GetAuthorizationByCode(authorizationRequestDto.AuthCode);
        var group         = await AuthorizationManager.GetGroupBaseById(authorizationRequestDto.EntityId);
        var saved         = await AuthorizationManager.AddAuthorizationToGroup(group, authorization, cancellationToken);
        return new PostOkResult(saved.Id.ToString());
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [Route("groups")]
    public async Task<IActionResult> PostGroups(AuthorizationGroupsRequestDto authorizationGroupsRequestDto, CancellationToken cancellationToken)
    {
        await AuthorizationManager.AddAuthorizationToGroups(authorizationGroupsRequestDto, cancellationToken);
        return Ok();
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpGet("groups/ids")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<BaseDto>))]
    public async Task<IActionResult> GetByIds(int[] ids)
    {
        IList<GroupBase> groupBaseByIds = await AuthorizationManager.GetGroupBaseByIds(ids);
        return new EnumerableOkObjectResult(groupBaseByIds.Select(c => new BaseDto { Code = c.Code, Description = c.Description, Id = c.Id }));
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpDelete("groups/{id:int}")]
    public async Task<IActionResult> DeleteGroups(int id, CancellationToken cancellationToken)
    {
        await AuthorizationManager.RemoveAuthorizationFromGroup(id, cancellationToken);
        return NoContent();
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpGet("groups")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<BaseDto>))]
    public async Task<IActionResult> GetAll(string? q = null, CancellationToken cancellationToken = default)
    {
        IQueryable<GroupBase> groupBases = AuthorizationManager.Groups().Where(k => k.IsActive);
        if (!string.IsNullOrEmpty(q))
            groupBases = groupBases.Where(k => k.Description.Contains(q));

        List<GroupBase> groups = await groupBases.ToListAsync(cancellationToken);
        return new EnumerableOkObjectResult(groups.Select(c => new
        {
            Code        = c.Code,
            Description = c.Description,
            Id          = c.Id,
            isRole      = c is Role,
        }));
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpGet("groups/bycode")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BaseDto))]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        GroupBase byCode = await AuthorizationManager.Groups()
            .Where(k => k.IsActive && k.Code == code)
            .FirstOrDefaultAsync(cancellationToken);

        return new OkObjectResult(new { Code = byCode.Code, Description = byCode.Description, Id = byCode.Id, isRole = byCode is Role });
    }

    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpGet("groups/{id:int}/authorizations")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthorizationsByGroup(int id, CancellationToken cancellationToken)
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

    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [HttpGet("groups/bycodes")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BaseDto))]
    public async Task<IActionResult> GetByCodes(string codes, CancellationToken cancellationToken)
    {
        List<string> _codes = codes.Split(',').ToList();
        IQueryable<GroupBase> allGroupFounded = AuthorizationManager.Groups().Where(k => k.IsActive && _codes.Contains(k.Code));

        if (allGroupFounded != null && await allGroupFounded.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var listAsync = await allGroupFounded.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new EnumerableOkObjectResult(listAsync.Select(byCode => (object)new
            {
                Code = byCode.Code, Description = byCode.Description, Id = byCode.Id, isRole = byCode is Role,
            }));
        }

        return new EnumerableOkObjectResult(null);
    }
}
