using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Clients;
using DomuWave.Services.Clients.Request;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Dto.UserTenants;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Proxy per la gestione degli utenti: delega le operazioni CRUD all'AuthService
/// e arricchisce le risposte con i tenant DomuWave associati.
/// </summary>
[Route("api/users")]
public class UsersController(
    ILogger<UsersController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IAuthorizationClient authorizationClient,
    IUserTenantService userTenantService,
    ITenantService tenantService,
    IUserService userService,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)

{
    private readonly IMediator _mediator = mediator;
    // ─── GET /api/users/search ────────────────────────────────────────────────

    [HttpGet("search")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Users, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] string? roles,
        CancellationToken ct)
    {
        var users = await authorizationClient.SearchUsersAsync(
            CommonKeys.SystemUserToken, search, isActive, roles, ct);

        if (!this.CurrentUser.IsSystemUser)
        {
            var items = await _mediator.GetResponse(new GetUserTenantsByTenantCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct);
            
            users = users.Where(u => items.Any(k => k.UserId == u.Id && k.IsActive && !k.IsDeleted)).ToList();

        }

        return Ok(users ?? new List<CPQ.Core.DTO.UserDto>());
    }

    // ─── GET /api/users/{id} ──────────────────────────────────────────────────

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Users, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var user = await authorizationClient.GetUserByIdAsync(
            CommonKeys.SystemUserToken, id, ct);

        if (user == null) return NotFound();

        return Ok(user);
    }

    // ─── POST /api/users ──────────────────────────────────────────────────────

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Users, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAuthUserRequest dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // 1. Crea l'utente sull'auth service tramite IAuthorizationClient
        var created = await authorizationClient.CreateUserAsync(
            CommonKeys.SystemUserToken, dto, ct);

        // 2. Se il chiamante ha un tenant nel contesto, associa automaticamente
        //    il nuovo utente a quel tenant — senza delegare al client.
        if (created != null && TenantId.HasValue)
        {
            var currentUser = await userService.GetByIdAsync(CurrentUser.Id, ct)
                .ConfigureAwait(false);

            var tenant = await tenantService.GetByIdAsync(TenantId.Value, currentUser, ct)
                .ConfigureAwait(false);

            if (tenant != null)
            {
                var userTenant = new UserTenant
                {
                    UserId    = created.Id,
                    Tenant    = tenant,
                    IsDefault = false,
                    IsActive  = true,
                };
                userTenant.Trace(currentUser);

                await userTenantService.CreateAsync(userTenant, currentUser, ct)
                    .ConfigureAwait(false);
            }
        }

        return Ok(created);
    }

    // ─── PUT /api/users/{id} ──────────────────────────────────────────────────

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Users, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthUserRequest dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await authorizationClient.UpdateUserAsync(CommonKeys.SystemUserToken, id, dto, ct);

        return NoContent();
    }

    // ─── DELETE /api/users/{id} ───────────────────────────────────────────────

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Users, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await authorizationClient.DeleteUserAsync(CommonKeys.SystemUserToken, id, ct);
        return NoContent();
    }

    // ─── POST /api/users/{id}/reset-password ──────────────────────────────────

    [HttpPost("{id:int}/reset-password")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Users, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken ct)
    {
        await authorizationClient.ResetPasswordByIdAsync(CommonKeys.SystemUserToken, id, ct);
        return Ok(new { message = "Email di reset password inviata" });
    }

    // ─── POST /api/users/{id}/impersonate ─────────────────────────────────────

    [HttpPost("{id:int}/impersonate")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Users, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(DomuWave.Application.Models.UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Impersonate(int id, CancellationToken ct)
    {
        var caller = await userService.GetByIdAsync(CurrentUser.Id, ct).ConfigureAwait(false);
        if (!caller.IsSystemUser)
            return Forbid();

        var authUser = await authorizationClient.GetUserByIdAsync(CommonKeys.SystemUserToken, id, ct)
            .ConfigureAwait(false);
        if (authUser == null)
            return NotFound();

        var systemUser = await userService.GetByTokenAsync(CommonKeys.SystemUserToken, ct)
            .ConfigureAwait(false);

        var returnDto = new DomuWave.Application.Models.UserDto
        {
            FullName = authUser.FullName ?? $"{authUser.Name} {authUser.LastName}".Trim(),
            Id       = authUser.Id,
            LastName = authUser.LastName,
            Name     = authUser.Email ?? authUser.Name,
            Token    = authUser.Token,
            Role     = authUser.RoleCode,
            IsActive = authUser.IsActive,
        };

        var domainUser = await userService.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (domainUser != null)
        {
            returnDto.Profile = domainUser.IsSystemUser
                ? DomuWave.Application.Models.UserProfile.SuperAdmin
                : domainUser.Role?.Code?.ToLower() == "condomino"
                    ? DomuWave.Application.Models.UserProfile.User
                    : DomuWave.Application.Models.UserProfile.TenantAdministrator;
        }

        IList<UserTenant> tenants = await _mediator
            .GetResponse(new GetUserTenantsByUserCommand(CurrentUser.Id, id), ct)
            .ConfigureAwait(false);

        var tenantsDto    = tenants.Where(k => k.Tenant.IsActive && k.IsActive).Select(j => j.ToDto()).ToList();
        var defaultTenant = tenantsDto.FirstOrDefault(j => j.IsDefault) ?? tenantsDto.FirstOrDefault();

        if (defaultTenant != null)
        {
            returnDto.AvailableTenants = tenantsDto
                .Select(k => new DomuWave.Services.Dto.Tenant.TenantReadDto
                {
                    Code      = k.TenantCode,
                    Name      = k.TenantName,
                    Id        = k.TenantId,
                    IsPrimary = k.IsDefault,
                })
                .ToList();
            returnDto.Tenant = new DomuWave.Services.Dto.Tenant.TenantReadDto
            {
                Code      = defaultTenant.TenantCode,
                Name      = defaultTenant.TenantName,
                Id        = defaultTenant.TenantId,
                IsPrimary = defaultTenant.IsDefault,
            };
        }

        return Ok(returnDto);
    }

}
