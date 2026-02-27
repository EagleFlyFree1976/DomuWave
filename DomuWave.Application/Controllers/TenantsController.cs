using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Tenant;
using DomuWave.Services.Dto.Tenant;
using DomuWave.Services.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Extensions;
using DomuWave.Services.Models;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione dei Tenant della piattaforma.
/// Il controller si occupa esclusivamente del contratto HTTP:
/// traduce la request in un Command e la response dal risultato del mediator.
/// Tutta la logica di business risiede nei Consumer del progetto Services.
/// </summary>
[Route("api/[controller]")]
public class TenantsController(
    ILogger<TenantsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateAdminControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    // ─── GET /api/tenants ─────────────────────────────────────────────────────

    /// <summary>Restituisce tutti i tenant non eliminati.</summary>
    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<TenantReadDto>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.GetResponse(
            new GetAllTenantsCommand(CurrentUser.Id),
            cancellationToken);

        return Ok(result);
    }

    // ─── GET /api/tenants/active ──────────────────────────────────────────────

    /// <summary>Restituisce solo i tenant attivi.</summary>
    [HttpGet("active")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<TenantReadDto>))]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await _mediator.GetResponse(
            new GetActiveTenantsCommand(CurrentUser.Id),
            cancellationToken);

        return Ok(result);
    }

    // ─── GET /api/tenants/{id} ────────────────────────────────────────────────

    /// <summary>Restituisce un tenant per ID.</summary>
    [HttpGet("{id:guid}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.GetResponse(
            new GetTenantByIdCommand(CurrentUser.Id, id),
            cancellationToken);

        if (result == null) return NotFound();
        return Ok(result);
    }

    // ─── GET /api/tenants/paged ───────────────────────────────────────────────

    /// <summary>Restituisce i tenant con paginazione e filtri.</summary>
    [HttpGet("paged")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<TenantReadDto>))]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int    page      = 1,
        [FromQuery] int    pageSize  = 20,
        [FromQuery] string sortField = "name",
        [FromQuery] bool   sortOrder = true,
        [FromQuery] string search    = null,
        [FromQuery] bool?  isActive  = null,
        CancellationToken  cancellationToken = default)
    {
        var result = await _mediator.GetResponse(
            new GetPagedTenantsCommand(CurrentUser.Id)
            {
                PageNumber = page,
                PageSize   = pageSize,
                SortField  = sortField,
                Asc        = sortOrder,
                Search     = search,
                IsActive   = isActive,
            },
            cancellationToken);

        return Ok(result);
    }

    // ─── POST /api/tenants ────────────────────────────────────────────────────

    /// <summary>Crea un nuovo tenant.</summary>
    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Tenants, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TenantReadDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _mediator.GetResponse(
            new CreateTenantCommand(CurrentUser.Id, dto),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ─── PUT /api/tenants/{id} ────────────────────────────────────────────────

    /// <summary>Aggiorna un tenant esistente.</summary>
    [HttpPut("{id:guid}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Tenants, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _mediator.GetResponse(
            new UpdateTenantCommand(CurrentUser.Id, id, dto),
            cancellationToken);

        if (result == null) return NotFound();
        return Ok(result);
    }

    // ─── DELETE /api/tenants/{id} ─────────────────────────────────────────────

    /// <summary>Soft-delete di un tenant.</summary>
    [HttpDelete("{id:guid}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Tenants, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.GetResponse(
            new DeleteTenantCommand(CurrentUser.Id, id),
            cancellationToken);

        if (!deleted) return NotFound();
        return NoContent();
    }
}
