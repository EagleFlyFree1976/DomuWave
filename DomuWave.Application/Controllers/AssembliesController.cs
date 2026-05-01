using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Assembly;
using DomuWave.Services.Command.AssemblyAgendaItem;
using DomuWave.Services.Command.AssemblyAttendance;
using DomuWave.Services.Dto.Assembly;
using DomuWave.Services.Dto.AssemblyAgendaItem;
using DomuWave.Services.Dto.AssemblyAttendance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/assemblies")]
[Produces("application/json")]
public class AssembliesController(
    ILogger<AssembliesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    // ── Assembly ─────────────────────────────────────────────────────────────

    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(typeof(IList<AssemblyReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetAssembliesByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AssemblyReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetAssemblyByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssemblyReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateAssemblyDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateAssemblyCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AssemblyReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAssemblyDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateAssemblyCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteAssemblyCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/close")]
    [ProducesResponseType(typeof(AssemblyReadDto), 200)]
    public async Task<IActionResult> Close(int id, [FromBody] CloseAssemblyRequestDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new CloseAssemblyCommand(CurrentUser.Id, id, dto.ActualDate), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(AssemblyReadDto), 200)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new CancelAssemblyCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // ── Agenda Items ─────────────────────────────────────────────────────────

    [HttpGet("{assemblyId:int}/agenda-items")]
    [ProducesResponseType(typeof(IList<AssemblyAgendaItemReadDto>), 200)]
    public async Task<IActionResult> GetAgendaItems(int assemblyId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetAgendaItemsByAssemblyCommand(CurrentUser.Id, assemblyId), ct));

    [HttpPost("agenda-items")]
    [ProducesResponseType(typeof(AssemblyAgendaItemReadDto), 201)]
    public async Task<IActionResult> CreateAgendaItem([FromBody] CreateAssemblyAgendaItemDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateAssemblyAgendaItemCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPut("agenda-items/{id:int}")]
    [ProducesResponseType(typeof(AssemblyAgendaItemReadDto), 200)]
    public async Task<IActionResult> UpdateAgendaItem(int id, [FromBody] UpdateAssemblyAgendaItemDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateAssemblyAgendaItemCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("agenda-items/{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAgendaItem(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteAssemblyAgendaItemCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ── Attendances ───────────────────────────────────────────────────────────

    [HttpGet("{assemblyId:int}/attendances")]
    [ProducesResponseType(typeof(IList<AssemblyAttendanceReadDto>), 200)]
    public async Task<IActionResult> GetAttendances(int assemblyId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetAttendancesByAssemblyCommand(CurrentUser.Id, assemblyId), ct));

    [HttpPost("attendances")]
    [ProducesResponseType(typeof(AssemblyAttendanceReadDto), 201)]
    public async Task<IActionResult> CreateAttendance([FromBody] CreateAssemblyAttendanceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateAssemblyAttendanceCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPut("attendances/{id:int}")]
    [ProducesResponseType(typeof(AssemblyAttendanceReadDto), 200)]
    public async Task<IActionResult> UpdateAttendance(int id, [FromBody] UpdateAssemblyAttendanceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateAssemblyAttendanceCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("attendances/{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAttendance(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteAssemblyAttendanceCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public class CloseAssemblyRequestDto
{
    public DateTime ActualDate { get; set; }
}
