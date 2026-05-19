using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.Maintenance;
using DomuWave.Services.Dto.Maintenance;
using Microsoft.AspNetCore.Mvc;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/maintenance")]
[Produces("application/json")]
public class MaintenanceController(
    ILogger<MaintenanceController> logger,
    Microsoft.Extensions.Options.IOptionsMonitor<CPQ.Core.Settings.OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Maintenance, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MaintenanceReadDto>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMaintenanceByCondominiumCommand(CurrentUser.Id, condominiumId, TenantId.GetValueOrDefault()), ct));

    [HttpGet("condominium/{condominiumId:int}/open")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Maintenance, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MaintenanceReadDto>))]
    public async Task<IActionResult> GetOpen(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetOpenMaintenanceCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("condominium/{condominiumId:int}/status/{status}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Maintenance, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MaintenanceReadDto>))]
    public async Task<IActionResult> GetByStatus(int condominiumId, string status, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMaintenanceByStatusCommand(CurrentUser.Id, condominiumId, status), ct));

    [HttpGet("condominium/{condominiumId:int}/priority/{priority}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Maintenance, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MaintenanceReadDto>))]
    public async Task<IActionResult> GetByPriority(int condominiumId, string priority, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMaintenanceByPriorityCommand(CurrentUser.Id, condominiumId, priority), ct));

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Maintenance, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MaintenanceReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetMaintenanceByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Maintenance, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MaintenanceReadDto))]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateMaintenanceCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Maintenance, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MaintenanceReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMaintenanceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateMaintenanceCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Maintenance, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteMaintenanceCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
