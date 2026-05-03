using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.RealEstateUnit;
using DomuWave.Services.Command.UnitOpeningBalance;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.UnitOpeningBalance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

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

    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantId"]?.ToString() ?? Guid.Empty.ToString());

    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(typeof(IList<RealEstateUnitReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetRealEstateUnitsByCondominiumCommand(CurrentUser.Id, condominiumId, TenantGuid), ct));

    [HttpGet("by-condominium/{condominiumId:int}/panoramica")]
    [ProducesResponseType(typeof(IList<RealEstateUnitPanoramaDto>), 200)]
    public async Task<IActionResult> GetPanoramica(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetCondominiumPanoramaCommand(CurrentUser.Id, condominiumId, TenantGuid), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RealEstateUnitReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetRealEstateUnitByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-condominium/{condominiumId:int}/staircase/{staircase}")]
    [ProducesResponseType(typeof(IList<RealEstateUnitReadDto>), 200)]
    public async Task<IActionResult> GetByStaircase(int condominiumId, string staircase, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetRealEstateUnitsByStaircaseCommand(CurrentUser.Id, condominiumId, staircase), ct));

    [HttpGet("by-condominium/{condominiumId:int}/floor/{floor:int}")]
    [ProducesResponseType(typeof(IList<RealEstateUnitReadDto>), 200)]
    public async Task<IActionResult> GetByFloor(int condominiumId, int floor, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetRealEstateUnitsByFloorCommand(CurrentUser.Id, condominiumId, floor), ct));

    [HttpGet("by-condominium/{condominiumId:int}/type/{unitType}")]
    [ProducesResponseType(typeof(IList<RealEstateUnitReadDto>), 200)]
    public async Task<IActionResult> GetByType(int condominiumId, string unitType, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetRealEstateUnitsByTypeCommand(CurrentUser.Id, condominiumId, unitType), ct));

    [HttpGet("by-condominium/{condominiumId:int}/count")]
    public async Task<IActionResult> GetCount(int condominiumId, CancellationToken ct)
    {
        var count = await _mediator.GetResponse(new GetRealEstateUnitsCountCommand(CurrentUser.Id, condominiumId), ct);
        return Ok(new { condominiumId, count });
    }

    [HttpPost]
    [ProducesResponseType(typeof(RealEstateUnitReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateRealEstateUnitDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateRealEstateUnitCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RealEstateUnitReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRealEstateUnitDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateRealEstateUnitCommand(CurrentUser.Id, id, dto), ct);
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

    // ── Bilancio iniziale ──────────────────────────────────────────────────

    [HttpGet("opening-balances/by-fiscal-year/{fiscalYearId:int}")]
    [ProducesResponseType(typeof(IList<UnitOpeningBalanceReadDto>), 200)]
    public async Task<IActionResult> GetOpeningBalancesByFiscalYear(int fiscalYearId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetUnitOpeningBalancesByFiscalYearCommand(CurrentUser.Id, fiscalYearId), ct);
        return Ok(result ?? []);
    }

    [HttpGet("{unitId:int}/opening-balance")]
    [ProducesResponseType(typeof(UnitOpeningBalanceReadDto), 200)]
    public async Task<IActionResult> GetOpeningBalance(int unitId, [FromQuery] int fiscalYearId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetUnitOpeningBalanceCommand(CurrentUser.Id, unitId, fiscalYearId), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("opening-balances/bulk")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SetOpeningBalancesBulk([FromBody] SetUnitOpeningBalancesBulkDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _mediator.GetResponse(new SetUnitOpeningBalancesBulkCommand(CurrentUser.Id, dto), ct);
        return NoContent();
    }

    [HttpPut("{unitId:int}/opening-balance")]
    [ProducesResponseType(typeof(UnitOpeningBalanceReadDto), 200)]
    public async Task<IActionResult> SetOpeningBalance(int unitId, [FromBody] SetUnitOpeningBalanceDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new SetUnitOpeningBalanceCommand(CurrentUser.Id, unitId, dto), ct);
        return Ok(result);
    }
}
