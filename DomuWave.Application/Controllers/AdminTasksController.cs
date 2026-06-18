using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Memberships;
using DomuWave.Application.Code;
using DomuWave.Services.Clients;
using DomuWave.Services.Command.AdminTask;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Dto.AdminTask;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/tasks")]
[Produces("application/json")]
public class AdminTasksController(
    ILogger<AdminTasksController> logger,
    Microsoft.Extensions.Options.IOptionsMonitor<CPQ.Core.Settings.OxCoreSettings> configuration,
    IAuthorizationClient authorizationClient,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.AdminTask, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<AdminTaskReadDto>))]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? assignedToUserId,
        [FromQuery] int? status,
        [FromQuery] DateTime? dueBefore,
        CancellationToken ct)
        => Ok(await _mediator.GetResponse(
            new GetAdminTasksByTenantCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), assignedToUserId, status, dueBefore), ct));

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.AdminTask, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminTaskReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetAdminTaskByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.AdminTask, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AdminTaskReadDto))]
    public async Task<IActionResult> Create([FromBody] CreateAdminTaskDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.AssignedToUserId.HasValue)
        {
            var fullName = await ResolveAssigneeFullNameAsync(dto.AssignedToUserId.Value, ct);
            if (fullName == null) return BadRequest("L'assegnatario selezionato non è un collaboratore valido di questo studio.");
            dto.AssignedToFullName = fullName;
        }

        var result = await _mediator.GetResponse(
            new CreateAdminTaskCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.AdminTask, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminTaskReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAdminTaskDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.AssignedToUserId.HasValue)
        {
            var fullName = await ResolveAssigneeFullNameAsync(dto.AssignedToUserId.Value, ct);
            if (fullName == null) return BadRequest("L'assegnatario selezionato non è un collaboratore valido di questo studio.");
            dto.AssignedToFullName = fullName;
        }
        else
        {
            dto.AssignedToFullName = null;
        }

        var result = await _mediator.GetResponse(new UpdateAdminTaskCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:int}/complete")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.AdminTask, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminTaskReadDto))]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new CompleteAdminTaskCommand(CurrentUser.Id, id), ct));

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.AdminTask, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteAdminTaskCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // Verifica che l'utente sia un collaboratore (CLB) attivo del tenant corrente e
    // ne restituisce il nome completo; null se non valido. Pattern da UsersController.Search.
    private async Task<string?> ResolveAssigneeFullNameAsync(int userId, CancellationToken ct)
    {
        // L'utente può sempre assegnare l'attività a sé stesso (oltre che ai collaboratori).
        if (userId == CurrentUser.Id)
        {
            return !string.IsNullOrWhiteSpace(CurrentUser.FullName)
                ? CurrentUser.FullName
                : $"{CurrentUser.FirstName} {CurrentUser.LastName}".Trim();
        }

        var users = await authorizationClient.SearchUsersAsync(
            CommonKeys.SystemUserToken, search: null, isActive: true, roles: "CLB", ct);

        var candidate = users?.FirstOrDefault(u => u.Id == userId);
        if (candidate == null) return null;

        if (!CurrentUser.IsSystemUser)
        {
            var tenantUsers = await _mediator.GetResponse(
                new GetUserTenantsByTenantCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct);
            var belongs = tenantUsers.Any(k => k.UserId == userId && k.IsActive && !k.IsDeleted);
            if (!belongs) return null;
        }

        return !string.IsNullOrWhiteSpace(candidate.FullName)
            ? candidate.FullName
            : $"{candidate.Name} {candidate.LastName}".Trim();
    }
}
