using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Consumption;
using DomuWave.Services.Dto.Consumption;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/consumption")]
[Produces("application/json")]
public class ConsumptionController(
    ILogger<ConsumptionController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    // ── ConsumptionType ───────────────────────────────────────────────────

    [HttpGet("types/by-condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ConsumptionTypeReadDto>), 200)]
    public async Task<IActionResult> GetTypesByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetConsumptionTypesByCondominiumCommand(CurrentUser.Id, condominiumId), ct) ?? []);

    [HttpGet("types/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsumptionTypeReadDto), 200)]
    public async Task<IActionResult> GetTypeById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetConsumptionTypeByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("types")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsumptionTypeReadDto), 201)]
    public async Task<IActionResult> CreateType([FromBody] CreateConsumptionTypeDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateConsumptionTypeCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetTypeById), new { id = result.Id }, result);
    }

    [HttpPut("types/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsumptionTypeReadDto), 200)]
    public async Task<IActionResult> UpdateType(int id, [FromBody] UpdateConsumptionTypeDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateConsumptionTypeCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("types/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> DeleteType(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new DeleteConsumptionTypeCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    // ── Meter ─────────────────────────────────────────────────────────────

    [HttpGet("meters/by-condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<MeterReadDto>), 200)]
    public async Task<IActionResult> GetMetersByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMetersByCondominiumCommand(CurrentUser.Id, condominiumId), ct) ?? []);

    [HttpGet("meters/by-type/{consumptionTypeId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<MeterReadDto>), 200)]
    public async Task<IActionResult> GetMetersByType(int consumptionTypeId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMetersByConsumptionTypeCommand(CurrentUser.Id, consumptionTypeId), ct) ?? []);

    [HttpGet("meters/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(MeterReadDto), 200)]
    public async Task<IActionResult> GetMeterById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetMeterByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("meters")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(MeterReadDto), 201)]
    public async Task<IActionResult> CreateMeter([FromBody] CreateMeterDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateMeterCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetMeterById), new { id = result.Id }, result);
    }

    [HttpPut("meters/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(MeterReadDto), 200)]
    public async Task<IActionResult> UpdateMeter(int id, [FromBody] UpdateMeterDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateMeterCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("meters/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> DeleteMeter(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new DeleteMeterCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    // ── ConsumptionReading ────────────────────────────────────────────────

    [HttpGet("readings/by-type/{consumptionTypeId:int}/fiscal-year/{fiscalYearId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ConsumptionReadingReadDto>), 200)]
    public async Task<IActionResult> GetReadings(int consumptionTypeId, int fiscalYearId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetConsumptionReadingsByFiscalYearCommand(CurrentUser.Id, consumptionTypeId, fiscalYearId), ct) ?? []);

    [HttpPut("readings/bulk")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ConsumptionReadingReadDto>), 200)]
    public async Task<IActionResult> SaveReadingsBulk([FromBody] SaveReadingsBulkRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new SaveConsumptionReadingsBulkCommand(CurrentUser.Id, dto.ConsumptionTypeId, dto.FiscalYearId, dto.Items), ct);
        return Ok(result);
    }

    [HttpDelete("readings/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> DeleteReading(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new DeleteConsumptionReadingCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    // ── ConsumptionCharge ─────────────────────────────────────────────────

    [HttpGet("readings/uncharged-count/by-fiscal-year/{fiscalYearId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> GetUnchargedReadingsCount(int fiscalYearId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetUnchargedReadingsCountByFiscalYearCommand(CurrentUser.Id, fiscalYearId), ct));

    [HttpGet("charges/by-fiscal-year/{fiscalYearId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ConsumptionChargeReadDto>), 200)]
    public async Task<IActionResult> GetCharges(int fiscalYearId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetConsumptionChargesByFiscalYearCommand(CurrentUser.Id, fiscalYearId), ct) ?? []);

    [HttpGet("charges/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsumptionChargeReadDto), 200)]
    public async Task<IActionResult> GetChargeById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetConsumptionChargeByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("charges")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsumptionChargeReadDto), 201)]
    public async Task<IActionResult> CreateCharge([FromBody] CreateConsumptionChargeDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateConsumptionChargeCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetChargeById), new { id = result.Id }, result);
    }

    [HttpPut("charges/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsumptionChargeReadDto), 200)]
    public async Task<IActionResult> UpdateCharge(int id, [FromBody] UpdateConsumptionChargeDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateConsumptionChargeCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("charges/{id:int}/recalculate")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsumptionChargeReadDto), 200)]
    public async Task<IActionResult> Recalculate(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new RecalculateConsumptionChargeCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("charges/{id:int}/approve")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsumptionChargeReadDto), 200)]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new ApproveConsumptionChargeCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("charges/{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> DeleteCharge(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new DeleteConsumptionChargeCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }
}

/// <summary>Wrapper per il bulk delle letture (evita parametri multipli nella route).</summary>
public class SaveReadingsBulkRequestDto
{
    public int ConsumptionTypeId { get; set; }
    public int FiscalYearId      { get; set; }
    public IList<UpsertConsumptionReadingDto> Items { get; set; } = [];
}
