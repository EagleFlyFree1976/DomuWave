using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.UnitTenant;
using DomuWave.Services.Dto.UnitTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/unit-tenants")]
[Produces("application/json")]
public class UnitTenantsController(
    ILogger<UnitTenantsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantId"]?.ToString() ?? Guid.Empty.ToString());

    [HttpGet("search")]
    [ProducesResponseType(typeof(IList<UnitTenantReadDto>), 200)]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new SearchUnitTenantsCommand(CurrentUser.Id, q ?? string.Empty, TenantId.GetValueOrDefault()), ct));

    [HttpGet("by-unit/{unitId:int}")]
    [ProducesResponseType(typeof(IList<UnitTenantReadDto>), 200)]
    public async Task<IActionResult> GetByUnit(int unitId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetUnitTenantsByUnitCommand(CurrentUser.Id, unitId, TenantGuid), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UnitTenantReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetUnitTenantByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UnitTenantReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateUnitTenantDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateUnitTenantCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UnitTenantReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitTenantDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateUnitTenantCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(UnitTenantReadDto), 200)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new DeleteUnitTenantCommand(CurrentUser.Id, id), ct);
        if (result == null) return NoContent();  // hard deleted
        return Ok(result);                       // soft deactivated
    }
}
