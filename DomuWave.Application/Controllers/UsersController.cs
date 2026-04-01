using Auth.Services.Command;
using Auth.Services.Extensions;
using Auth.Services.Interfaces;
using Auth.Services.Orchestators;
using CPQ.Core.Extensions;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Clients.Request;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione degli utenti tramite Auth.Services locale.
/// </summary>
[Route("api/users")]
public class UsersController(
    ILogger<UsersController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IAuthUserService authUserService,
    AuthOrchestator authOrchestator,
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
        var roleArray = roles?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var users = await authUserService.FindUser(search ?? string.Empty, roleArray, null, isActive, ct)
            .ConfigureAwait(false);

        var dtos = users.Select(u => u.ToDto()).ToList();

        if (!CurrentUser.IsSystemUser)
        {
            var items = await _mediator.GetResponse(new GetUserTenantsByTenantCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct);
            dtos = dtos.Where(u => items.Any(k => k.UserId == u.Id && k.IsActive && !k.IsDeleted)).ToList();
        }

        return Ok(dtos);
    }

    // ─── GET /api/users/{id} ──────────────────────────────────────────────────

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var user = await authUserService.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (user == null) return NotFound();
        return Ok(user.ToDto());
    }

    // ─── POST /api/users ──────────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAuthUserRequest dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await authOrchestator.CreateUser(new CreateUser
        {
            Email      = dto.Email,
            Name       = dto.Name,
            SurName    = dto.SurName,
            Password   = dto.Password,
            RoleCode   = dto.RoleCode,
            ModuleCode = dto.ModuleCode,
        }, ct).ConfigureAwait(false);

        if (created != null && TenantId.HasValue)
        {
            var currentUser = await userService.GetByIdAsync(CurrentUser.Id, ct).ConfigureAwait(false);
            var tenant = await tenantService.GetByIdAsync(TenantId.Value, currentUser, ct).ConfigureAwait(false);

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
                await userTenantService.CreateAsync(userTenant, currentUser, ct).ConfigureAwait(false);
            }
        }

        return Ok(created?.ToDto());
    }

    // ─── PUT /api/users/{id} ──────────────────────────────────────────────────

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthUserRequest dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await authUserService.UpdateUser(id, new UpdateUser
        {
            Email   = dto.Email,
            Name    = dto.Name,
            SurName = dto.SurName,
            IsActive = dto.IsActive,
            RoleCode = dto.RoleCode,
        }, ct).ConfigureAwait(false);

        return NoContent();
    }

    // ─── DELETE /api/users/{id} ───────────────────────────────────────────────

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var user = await authUserService.GetByIdAsync(id, ct).ConfigureAwait(false);
        if (user == null) return NotFound();
        user.IsDeleted = true;
        await authUserService.UpdateUser(id, new UpdateUser
        {
            Email    = user.Email,
            Name     = user.FirstName,
            SurName  = user.LastName,
            IsActive = false,
            RoleCode = user.Role?.Code,
        }, ct).ConfigureAwait(false);
        return NoContent();
    }

    // ─── POST /api/users/{id}/reset-password ──────────────────────────────────

    [HttpPost("{id:int}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken ct)
    {
        // reset-password is handled by AuthOrchestator.GeneratePasswordResetAsync via email
        await authOrchestator.GeneratePasswordResetAsync(
            (await authUserService.GetByIdAsync(id, ct).ConfigureAwait(false))?.Email ?? string.Empty,
            ct).ConfigureAwait(false);
        return Ok(new { message = "Email di reset password inviata" });
    }
}
