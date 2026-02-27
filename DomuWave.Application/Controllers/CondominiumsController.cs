using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Extensions;
namespace DomuWave.Microservice.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class CondominiumsController(
    ILogger<CondominiumsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantGuid"]?.ToString() ?? Guid.Empty.ToString());

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Condominium>))]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetAllCondominiumsCommand(CurrentUser.Id, TenantGuid), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Condominium))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Condominium>))]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetActiveCondominiumsCommand(CurrentUser.Id, TenantGuid), ct);
        return Ok(result);
    }

    [HttpGet("by-code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Condominium))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumByCodeCommand(CurrentUser.Id, TenantGuid, code), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("upcoming-assembly")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Condominium>))]
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
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Condominium))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Condominium condominium, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new CreateCondominiumCommand(CurrentUser.Id, condominium), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Condominium))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Condominium condominium, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateCondominiumCommand(CurrentUser.Id, id, condominium), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(
            new DeleteCondominiumCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
