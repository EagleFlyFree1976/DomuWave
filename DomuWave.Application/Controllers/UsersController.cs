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
            var user = users.Where(k => k.Id == 356).FirstOrDefault();
            users = users.Where(u => items.Any(k => k.UserId == u.Id && k.IsActive && !k.IsDeleted)).ToList();

        }

        return Ok(users ?? new List<CPQ.Core.DTO.UserDto>());
    }

    // ─── GET /api/users/{id} ──────────────────────────────────────────────────

    [HttpGet("{id:int}")]
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAuthUserRequest dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // 1. Crea l'utente sull'auth service tramite Refit
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await authorizationClient.DeleteUserAsync(CommonKeys.SystemUserToken, id, ct);
        return NoContent();
    }

    // ─── POST /api/users/{id}/reset-password ──────────────────────────────────

    [HttpPost("{id:int}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken ct)
    {
        await authorizationClient.ResetPasswordByIdAsync(CommonKeys.SystemUserToken, id, ct);
        return Ok(new { message = "Email di reset password inviata" });
    }
 
}
