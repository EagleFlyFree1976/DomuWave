using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.UnitOwner;
using DomuWave.Services.Dto.UnitOwner;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/unit-owners")]
[Produces("application/json")]
public class UnitOwnersController(
    ILogger<UnitOwnersController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-unit/{unitId:int}")]
    [ProducesResponseType(typeof(IList<UnitOwnerReadDto>), 200)]
    public async Task<IActionResult> GetByUnit(int unitId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetUnitOwnersByUnitCommand(CurrentUser.Id, unitId), ct));

    [HttpGet("by-user/{userId:long}")]
    [ProducesResponseType(typeof(IList<UserUnitOwnerDto>), 200)]
    public async Task<IActionResult> GetByUser(long userId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetUnitOwnersByUserCommand(CurrentUser.Id, userId), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UnitOwnerReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetUnitOwnerByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UnitOwnerReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateUnitOwnerDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateUnitOwnerCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UnitOwnerReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitOwnerDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateUnitOwnerCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(UnitOwnerReadDto), 200)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new DeleteUnitOwnerCommand(CurrentUser.Id, id), ct);
        if (result == null) return NoContent();  // hard deleted
        return Ok(result);                       // soft deactivated
    }
}
