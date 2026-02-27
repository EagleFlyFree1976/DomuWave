using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Extensions;
namespace DomuWave.Microservice.Controllers;

[Route("api/real-estate-units")]
[Produces("application/json")]
public class RealEstateUnitsController(
    ILogger<RealEstateUnitsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetRealEstateUnitsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetRealEstateUnitByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-condominium/{condominiumId:int}/staircase/{staircase}")]
    public async Task<IActionResult> GetByStaircase(int condominiumId, string staircase, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetRealEstateUnitsByStaircaseCommand(CurrentUser.Id, condominiumId, staircase), ct));

    [HttpGet("by-condominium/{condominiumId:int}/floor/{floor:int}")]
    public async Task<IActionResult> GetByFloor(int condominiumId, int floor, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetRealEstateUnitsByFloorCommand(CurrentUser.Id, condominiumId, floor), ct));

    [HttpGet("by-condominium/{condominiumId:int}/type/{unitType}")]
    public async Task<IActionResult> GetByType(int condominiumId, string unitType, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetRealEstateUnitsByTypeCommand(CurrentUser.Id, condominiumId, unitType), ct));

    [HttpGet("by-condominium/{condominiumId:int}/count")]
    public async Task<IActionResult> GetCount(int condominiumId, CancellationToken ct)
    {
        var count = await _mediator.GetResponse(new GetRealEstateUnitsCountCommand(CurrentUser.Id, condominiumId), ct);
        return Ok(new { condominiumId, count });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RealEstateUnit unit, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateRealEstateUnitCommand(CurrentUser.Id, unit), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RealEstateUnit unit, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateRealEstateUnitCommand(CurrentUser.Id, id, unit), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteRealEstateUnitCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
