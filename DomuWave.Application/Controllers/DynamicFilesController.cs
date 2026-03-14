using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.DynamicFile;
using DomuWave.Services.Dto.DynamicFile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/dynamic-files")]
[Produces("application/json")]
public class DynamicFilesController(
    ILogger<DynamicFilesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantId"]?.ToString() ?? Guid.Empty.ToString());

    /// <summary>
    /// Restituisce la lista dei file collegati a un'entita'.
    /// Usa entityName per ricerca semplice o entityFullName per ricerca esatta sul tipo completo.
    /// </summary>
    [HttpGet("by-entity")]
    [ProducesResponseType(typeof(IList<DynamicFileReadDto>), 200)]
    public async Task<IActionResult> GetByEntity(
        [FromQuery] string entityName,
        [FromQuery] int    entityId,
        [FromQuery] string? entityFullName,
        CancellationToken ct)
        => Ok(await _mediator.GetResponse(
            new GetDynamicFilesByEntityCommand(CurrentUser.Id, entityName, entityFullName ?? string.Empty, entityId), ct));

    /// <summary>Restituisce i metadati del file (senza contenuto base64).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DynamicFileReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetDynamicFileCommand(CurrentUser.Id, id, includeContent: false), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Restituisce i metadati + contenuto base64 del file (per download).</summary>
    [HttpGet("{id:int}/download")]
    [ProducesResponseType(typeof(DynamicFileReadDto), 200)]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetDynamicFileCommand(CurrentUser.Id, id, includeContent: true), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Carica un nuovo file nel repository.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DynamicFileReadDto), 201)]
    public async Task<IActionResult> Upload([FromBody] UploadDynamicFileDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UploadDynamicFileCommand(CurrentUser.Id, TenantGuid, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Aggiorna descrizione e tag del file.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DynamicFileReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDynamicFileDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateDynamicFileCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Elimina un file (soft delete + pulizia storage esterno se presente).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteDynamicFileCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
