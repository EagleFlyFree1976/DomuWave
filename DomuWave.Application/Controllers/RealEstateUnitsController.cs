using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Domain.Models;
using DomuWave.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione Unità Immobiliari (appartamenti, garage, box, ecc.).
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class RealEstateUnitsController(
    ILogger<RealEstateUnitsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IRealEstateUnitService unitService)
    : PrivateControllerBase(logger, configuration)
{
    /// <summary>
    /// Restituisce tutte le unità di un condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<RealEstateUnit>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await unitService.GetByCondominiumIdAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce una singola unità per ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RealEstateUnit))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await unitService.GetByIdAsync(id, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Filtra le unità per scala.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/staircase/{staircase}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<RealEstateUnit>))]
    public async Task<IActionResult> GetByStaircase(int condominiumId, string staircase, CancellationToken cancellationToken)
    {
        var result = await unitService.GetByStaircaseAsync(condominiumId, staircase, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Filtra le unità per piano.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/floor/{floor:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<RealEstateUnit>))]
    public async Task<IActionResult> GetByFloor(int condominiumId, int floor, CancellationToken cancellationToken)
    {
        var result = await unitService.GetByFloorAsync(condominiumId, floor, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Filtra le unità per tipo (Appartamento, Garage, Box, ecc.).
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/type/{unitType}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<RealEstateUnit>))]
    public async Task<IActionResult> GetByType(int condominiumId, string unitType, CancellationToken cancellationToken)
    {
        var result = await unitService.GetByTypeAsync(condominiumId, unitType, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce il conteggio totale delle unità del condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/count")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public async Task<IActionResult> GetCount(int condominiumId, CancellationToken cancellationToken)
    {
        var count = await unitService.GetTotalUnitsCountAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(new { condominiumId, count });
    }

    /// <summary>
    /// Crea una nuova unità immobiliare.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RealEstateUnit))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RealEstateUnit unit, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await unitService.CreateAsync(unit, CurrentUser, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Aggiorna un'unità immobiliare esistente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RealEstateUnit))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] RealEstateUnit unit, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var exists = await unitService.ExistsAsync(id, CurrentUser, cancellationToken);
        if (!exists) return NotFound();
        unit.Id = id;
        var result = await unitService.UpdateAsync(unit, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Rimuove (soft-delete) un'unità immobiliare.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await unitService.DeleteAsync(id, CurrentUser, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}



