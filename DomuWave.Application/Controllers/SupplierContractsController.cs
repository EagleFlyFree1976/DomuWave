using CPQ.Core.Extensions;
using DomuWave.Application.Code;
using DomuWave.Services.Command.SupplierContract;
using DomuWave.Services.Dto.SupplierContract;
using Microsoft.AspNetCore.Mvc;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/supplier-contracts")]
[Produces("application/json")]
public class SupplierContractsController(
    ILogger<SupplierContractsController> logger,
    Microsoft.Extensions.Options.IOptionsMonitor<CPQ.Core.Settings.OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("condominium/{condominiumId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<SupplierContractReadDto>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetContractsByCondominiumCommand(CurrentUser.Id, condominiumId, TenantId.GetValueOrDefault()), ct));

    [HttpGet("supplier/{supplierId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<SupplierContractReadDto>))]
    public async Task<IActionResult> GetBySupplier(int supplierId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetContractsBySupplierCommand(CurrentUser.Id, supplierId), ct));

    [HttpGet("condominium/{condominiumId:int}/active")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<SupplierContractReadDto>))]
    public async Task<IActionResult> GetActive(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetActiveContractsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("condominium/{condominiumId:int}/expiring")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<SupplierContractReadDto>))]
    public async Task<IActionResult> GetExpiring(
        int condominiumId,
        [FromQuery] int days = 30,
        CancellationToken ct = default)
        => Ok(await _mediator.GetResponse(new GetExpiringContractsByCondominiumCommand(CurrentUser.Id, condominiumId, days), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SupplierContractReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetContractByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SupplierContractReadDto))]
    public async Task<IActionResult> Create([FromBody] CreateSupplierContractDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateSupplierContractCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SupplierContractReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierContractDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateSupplierContractCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteSupplierContractCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
