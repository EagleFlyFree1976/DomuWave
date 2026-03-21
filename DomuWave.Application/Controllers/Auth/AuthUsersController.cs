using System.ComponentModel.DataAnnotations;
using Auth.Services.Command;
using Auth.Services.Extensions;
using Auth.Services.Interfaces;
using Auth.Services.Models;
using Auth.Services.Models.Dto;
using Auth.Services.Orchestators;
using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.ModelBinders;
using CPQ.Core.Result;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Microservice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using GroupBase = Auth.Services.Models.GroupBase;
using User = Auth.Services.Models.User;

namespace DomuWave.Microservice.Controllers;

[Route("api/users")]
public class AuthUsersController(
    ILogger<UsersController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IUserService userService,
    AuthOrchestator authOrchestator,
    IAuthUserService authUserService,
    IMemoryCache cache)
    : AuthorizationBaseController(logger, configuration)
{
    protected readonly AuthOrchestator _authOrchestator = authOrchestator;
    protected readonly IAuthUserService _authUserService = authUserService;
    private readonly IMemoryCache _cache = cache;
    protected readonly IUserService _userService = userService;

    [HttpGet]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<BaseDto>))]
    public async Task<IActionResult> Get(string search, CancellationToken cancellationToken)
    {
        var users = await _userService.Find(search, cancellationToken).ConfigureAwait(false);
        var userList = users.ToList().Select(o => o.ToDto()).ToList();
        return new EnumerableOkObjectResult(userList);
    }

    [Route("search")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<BaseDto>))]
    public async Task<IActionResult> Search(
        string search,
        [ModelBinder(BinderType = typeof(ArrayModelBinder))] string[] roles,
        [ModelBinder(BinderType = typeof(ArrayModelBinder))] string[] groups,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var users = await _authUserService.FindUser(search, roles, groups, isActive, cancellationToken);

        var flatUsers = users.Select(p => new AuthFlatUser
        {
            Email      = p.Email,
            FirstName  = p.FirstName,
            Id         = p.Id,
            IsActive   = p.IsActive,
            LastName   = p.LastName,
            Name       = p.Name,
            RoleCode   = p.Role.Code,
            Role       = p.Role.Description,
            TzDislpayName = "it",
            Path       = p.Path,
        });
        return new EnumerableOkObjectResult(flatUsers);
    }

    [HttpPost("register")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<BaseDto>))]
    public async Task<IActionResult> PostSales(CreateUser user, CancellationToken cancellationToken)
    {
        var createdUser = await _authOrchestator.CreateUser(user, cancellationToken).ConfigureAwait(false);
        return Ok(createdUser.ToDto());
    }

    [Route("defaultfilters")]
    public async Task<IActionResult> GetDefaultStatusFilter(string entityfullname, CancellationToken cancellationToken)
    {
        IList<GroupBase> allUserGroups = new List<GroupBase>();
        User currentUser = (User)CurrentUser;
        allUserGroups.Add(currentUser.Role);
        foreach (var group in currentUser.Groups) allUserGroups.Add(group);

        var defaultStatusFilters = await _authUserService.Find(allUserGroups.ToList(), currentUser, entityfullname, cancellationToken);
        return new EnumerableOkObjectResult(defaultStatusFilters);
    }

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(BaseDto))]
    public async Task<IActionResult> GetByID(int id, CancellationToken cancellationToken)
    {
        CPQ.Core.Memberships.User user = await _userService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (user == null) return NotFound();
        var dto = user.ToDto();
        return Ok(new { dto.Id, dto.Name, dto.LastName, dto.FullName, dto.Email, dto.Login, Role = user.Role.Description, RoleCode = user.Role.Code, dto.Token, IsActive = user?.IsActive });
    }

    [HttpPost("{id:int}/reset-password")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken cancellationToken)
    {
        await _authOrchestator.GeneratePasswordResetAsync(id, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateUser command, CancellationToken cancellationToken)
    {
        await _authUserService.UpdateUser(id, command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("bytoken")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanAction, "Admin", AuthorizationKeys.Module)]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(User))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByToken([FromBody][Required] TokenRequest tokenRequest, CancellationToken cancellationToken)
    {
        User user = (User)await _userService.GetByTokenAsync(tokenRequest.Token, cancellationToken).ConfigureAwait(false);
        var userInfo = user.toJSon().parseJson<User>();
        return Ok(userInfo);
    }
}
