using CPQ.Core.Extensions;
using DomuWave.Application.Code;
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
    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantId"]?.ToString() ?? Guid.Empty.ToString());

    [HttpGet("condominium/{condominiumId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MaintenanceReadDto>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMaintenanceByCondominiumCommand(CurrentUser.Id, condominiumId, TenantGuid), ct));

    [HttpGet("condominium/{condominiumId:int}/open")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MaintenanceReadDto>))]
    public async Task<IActionResult> GetOpen(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetOpenMaintenanceCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("condominium/{condominiumId:int}/status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MaintenanceReadDto>))]
    public async Task<IActionResult> GetByStatus(int condominiumId, string status, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMaintenanceByStatusCommand(CurrentUser.Id, condominiumId, status), ct));

    [HttpGet("condominium/{condominiumId:int}/priority/{priority}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MaintenanceReadDto>))]
    public async Task<IActionResult> GetByPriority(int condominiumId, string priority, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMaintenanceByPriorityCommand(CurrentUser.Id, condominiumId, priority), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MaintenanceReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetMaintenanceByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MaintenanceReadDto))]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateMaintenanceCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteMaintenanceCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
