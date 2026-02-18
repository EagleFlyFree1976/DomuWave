using DomuWave.Application.Code;
using DomuWave.Domain.Models;
using DomuWave.Services.Interfaces;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione Condomini - operazioni CRUD e query specializzate.
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class CondominiumsController(
    ILogger<CondominiumsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    ICondominiumService condominiumService)
    : PrivateControllerBase(logger, configuration)
{
    // ─── READ ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Restituisce tutti i condomini del tenant corrente.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Condominium>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await condominiumService.GetByTenantIdAsync(TenantGuid, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce un condominio per ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Condominium))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await condominiumService.GetByIdAsync(id, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Restituisce solo i condomini attivi del tenant.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Condominium>))]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await condominiumService.GetActiveCondominiumsAsync(TenantGuid, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce un condominio per codice.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Condominium))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var result = await condominiumService.GetByCodeAsync(TenantGuid, code, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Restituisce condomini con assemblee imminenti.
    /// </summary>
    [HttpGet("upcoming-assembly")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Condominium>))]
    public async Task<IActionResult> GetWithUpcomingAssembly([FromQuery] int daysAhead = 30, CancellationToken cancellationToken = default)
    {
        var result = await condominiumService.GetCondominiumsWithUpcomingAssemblyAsync(TenantGuid, daysAhead, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lista paginata di condomini.
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await condominiumService.GetPagedAsync(
            page, pageSize,
            x => !x.IsDeleted,
            x => x.Name!,
            ascending,
            CurrentUser,
            cancellationToken);

        return Ok(new { items, total, page, pageSize });
    }

    // ─── WRITE ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuovo condominio.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Condominium))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Condominium condominium, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await condominiumService.CreateAsync(condominium, CurrentUser, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Aggiorna un condominio esistente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Condominium))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] Condominium condominium, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var exists = await condominiumService.ExistsAsync(id, CurrentUser, cancellationToken);
        if (!exists) return NotFound();

        condominium.Id = id;
        var result = await condominiumService.UpdateAsync(condominium, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Soft-delete di un condominio.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await condominiumService.DeleteAsync(id, CurrentUser, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantGuid"]?.ToString() ?? Guid.Empty.ToString());
}
