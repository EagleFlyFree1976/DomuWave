using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class CondominiumsController(
    ILogger<CondominiumsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantId"]?.ToString() ?? Guid.Empty.ToString());

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumReadDto>))]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetAllCondominiumsCommand(CurrentUser.Id, TenantGuid), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumReadDto>))]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetActiveCondominiumsCommand(CurrentUser.Id, TenantGuid), ct);
        return Ok(result);
    }

    [HttpGet("by-code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumByCodeCommand(CurrentUser.Id, TenantGuid, code), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("upcoming-assembly")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumReadDto>))]
    public async Task<IActionResult> GetWithUpcomingAssembly(
        [FromQuery] int daysAhead = 30, CancellationToken ct = default)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumsWithUpcomingAssemblyCommand(CurrentUser.Id, TenantGuid, daysAhead), ct);
        return Ok(result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool ascending = true,
        CancellationToken ct = default)
    {
        var result = await _mediator.GetResponse(
            new GetPagedCondominiumsCommand(CurrentUser.Id, page, pageSize, ascending), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CondominiumReadDto))]
    public async Task<IActionResult> Create([FromBody] CreateCondominiumDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new CreateCondominiumCommand(CurrentUser.Id, TenantGuid, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCondominiumDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateCondominiumCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(
            new DeleteCondominiumCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("{id:int}/setup-status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumSetupStatusDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSetupStatus(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumSetupStatusCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
