using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

// ═══════════════════════════════════════════════════════════════════════════
// Fornitori
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Gestione Fornitori del condominio.
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class SuppliersController(
    ILogger<SuppliersController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    ISupplierService supplierService)
    : PrivateControllerBase(logger, configuration)
{
    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantGuid"]?.ToString() ?? Guid.Empty.ToString());

    /// <summary>
    /// Restituisce tutti i fornitori del tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Supplier>))]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await supplierService.GetByTenantIdAsync(TenantGuid, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce un fornitore per ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Supplier))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await supplierService.GetByIdAsync(id, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Cerca fornitori per partita IVA.
    /// </summary>
    [HttpGet("by-vat/{vatNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Supplier))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByVat(string vatNumber, CancellationToken cancellationToken)
    {
        var result = await supplierService.GetByVatNumberAsync(TenantGuid, vatNumber, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Filtra fornitori per tipo (Idraulico, Elettricista, ecc.).
    /// </summary>
    [HttpGet("by-type/{supplierType}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Supplier>))]
    public async Task<IActionResult> GetByType(string supplierType, CancellationToken cancellationToken)
    {
        var result = await supplierService.GetByTypeAsync(TenantGuid, supplierType, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Ricerca full-text fornitori.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Supplier>))]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest(new { error = "Il parametro 'q' è obbligatorio." });
        var result = await supplierService.SearchSuppliersAsync(TenantGuid, q, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuovo fornitore.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Supplier))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Supplier supplier, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await supplierService.CreateAsync(supplier, CurrentUser, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Aggiorna un fornitore esistente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Supplier))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Supplier supplier, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var exists = await supplierService.ExistsAsync(id, CurrentUser, cancellationToken);
        if (!exists) return NotFound();
        supplier.Id = id;
        var result = await supplierService.UpdateAsync(supplier, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Soft-delete di un fornitore.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await supplierService.DeleteAsync(id, CurrentUser, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Tabelle Millesimali
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Gestione Tabelle Millesimali del condominio.
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class MillesimalTablesController(
    ILogger<MillesimalTablesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMillesimalTableService millesimalTableService)
    : PrivateControllerBase(logger, configuration)
{
    /// <summary>
    /// Restituisce tutte le tabelle millesimali di un condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MillesimalTable>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await millesimalTableService.GetByCondominiumIdAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce una tabella per ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MillesimalTable))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await millesimalTableService.GetByIdAsync(id, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Cerca una tabella per codice.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MillesimalTable))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(int condominiumId, string code, CancellationToken cancellationToken)
    {
        var result = await millesimalTableService.GetByCodeAsync(condominiumId, code, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Restituisce solo le tabelle attive di un condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/active")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<MillesimalTable>))]
    public async Task<IActionResult> GetActive(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await millesimalTableService.GetActiveTablesAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea una nuova tabella millesimale.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MillesimalTable))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] MillesimalTable table, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await millesimalTableService.CreateAsync(table, CurrentUser, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Aggiorna una tabella millesimale esistente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MillesimalTable))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] MillesimalTable table, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var exists = await millesimalTableService.ExistsAsync(id, CurrentUser, cancellationToken);
        if (!exists) return NotFound();
        table.Id = id;
        var result = await millesimalTableService.UpdateAsync(table, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Soft-delete di una tabella millesimale.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await millesimalTableService.DeleteAsync(id, CurrentUser, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
