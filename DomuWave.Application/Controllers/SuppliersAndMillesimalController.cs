using CPQ.Core.ActionFilters;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.Supplier;
using DomuWave.Services.Command.MillesimalTable;
using DomuWave.Services.Command.UnitMillesimal;
using DomuWave.Services.Dto.Supplier;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Dto.UnitMillesimal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Extensions;
namespace DomuWave.Microservice.Controllers;

// ─── Suppliers ────────────────────────────────────────────────────────────────

[Route("api/suppliers")]
[Produces("application/json")]
public class SuppliersController(
    ILogger<SuppliersController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    
    [HttpGet]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Suppliers, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetAllSuppliersCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct));

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Suppliers, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetSupplierByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-vat/{vatNumber}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Suppliers, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetByVat(string vatNumber, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetSupplierByVatCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), vatNumber), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-type/{supplierType}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Suppliers, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetByType(string supplierType, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetSuppliersByTypeCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), supplierType), ct));

    [HttpGet("search")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Suppliers, Modules.DomuWaveModule)]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { error = "Il parametro 'q' è obbligatorio." });
        return Ok(await _mediator.GetResponse(new SearchSuppliersCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), q), ct));
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Suppliers, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(SupplierReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateSupplierCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Suppliers, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(SupplierReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateSupplierCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Suppliers, Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteSupplierCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

// ─── MillesimalTables ─────────────────────────────────────────────────────────

[Route("api/millesimal-tables")]
[Produces("application/json")]
public class MillesimalTablesController(
    ILogger<MillesimalTablesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    

    [HttpGet("by-condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.MillesimalTables, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetMillesimalTablesByCondominiumCommand(CurrentUser.Id, condominiumId, TenantId.GetValueOrDefault()), ct));

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.MillesimalTables, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetMillesimalTableByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-condominium/{condominiumId:int}/code/{code}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.MillesimalTables, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetByCode(int condominiumId, string code, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetMillesimalTableByCodeCommand(CurrentUser.Id, condominiumId, code), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-condominium/{condominiumId:int}/active")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.MillesimalTables, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetActive(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetActiveMillesimalTablesCommand(CurrentUser.Id, condominiumId), ct));

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.MillesimalTables, Modules.DomuWaveModule)]
    public async Task<IActionResult> Create([FromBody] CreateMillesimalTableDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateMillesimalTableCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.MillesimalTables, Modules.DomuWaveModule)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMillesimalTableDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateMillesimalTableCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{id:int}/enabled")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.MillesimalTables, Modules.DomuWaveModule)]
    public async Task<IActionResult> SetEnabled(int id, [FromBody] bool isEnabled, CancellationToken ct)
    {
        await _mediator.GetResponse(new SetMillesimalTableEnabledCommand(CurrentUser.Id, id, isEnabled), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.MillesimalTables, Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteMillesimalTableCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

// ─── UnitMillesimals ──────────────────────────────────────────────────────────

[Route("api/unit-millesimals")]
[Produces("application/json")]
public class UnitMillesimalsController(
    ILogger<UnitMillesimalsController> logger,
    IOptionsMonitor<OxCoreSettings>    configuration,
    IMediator                          mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-table/{tableId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.UnitMillesimals, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetByTable(int tableId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetUnitMillesimalsByTableCommand(CurrentUser.Id, tableId), ct));

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.UnitMillesimals, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(UnitMillesimalReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateUnitMillesimalDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateUnitMillesimalCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.UnitMillesimals, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(UnitMillesimalReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitMillesimalDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateUnitMillesimalCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.UnitMillesimals, Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteUnitMillesimalCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
