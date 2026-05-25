using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Condominium;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Models;
using LicenseManager.Client.Filters;
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
    [HttpGet]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumReadDto>))]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetAllCondominiumsCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumByIdCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("active")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumReadDto>))]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetActiveCondominiumsCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct);
        return Ok(result);
    }

    [HttpGet("by-code/{code}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumByCodeCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), code), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("upcoming-assembly")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumReadDto>))]
    public async Task<IActionResult> GetWithUpcomingAssembly(
        [FromQuery] int daysAhead = 30, CancellationToken ct = default)
    {
        var result = await _mediator.GetResponse(
            new GetCondominiumsWithUpcomingAssemblyCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), daysAhead), ct);
        return Ok(result);
    }

    [HttpGet("paged")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
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
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CondominiumReadDto))]
 
    public async Task<IActionResult> Create([FromBody] CreateCondominiumDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new CreateCondominiumCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCondominiumDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateCondominiumCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(
            new DeleteCondominiumCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("{id:int}/setup-status")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Condominiums, Modules.DomuWaveModule)]
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
