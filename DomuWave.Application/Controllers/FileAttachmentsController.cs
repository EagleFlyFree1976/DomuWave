using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.FileAttachment;
using DomuWave.Services.Dto.FileAttachment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/file-attachments")]
[Produces("application/json")]
public class FileAttachmentsController(
    ILogger<FileAttachmentsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    /// <summary>Restituisce la lista dei metadati allegati a una specifica entita' (senza base64)</summary>
    [HttpGet("by-entity/{entityKey}/{entityId:int}")]
    public async Task<IActionResult> GetByEntity(string entityKey, int entityId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(
            new GetFileAttachmentsByEntityCommand(CurrentUser.Id, entityKey, entityId), ct));

    /// <summary>Scarica un allegato specifico con il contenuto base64</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetFileAttachmentCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Carica un nuovo file allegato</summary>
    [HttpPost]
    [ProducesResponseType(typeof(FileAttachmentReadDto), 201)]
    public async Task<IActionResult> Upload([FromBody] UploadFileAttachmentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UploadFileAttachmentCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Elimina un allegato</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteFileAttachmentCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
