using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.Document;
using DomuWave.Services.Dto.Document;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/documents")]
[Produces("application/json")]
public class DocumentsController(
    ILogger<DocumentsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Documents, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<DocumentReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetDocumentsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Documents, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(DocumentReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetDocumentByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:int}/download")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Documents, Modules.DomuWaveModule)]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var (content, fileName, mimeType) = await _mediator.GetResponse(new DownloadDocumentCommand(CurrentUser.Id, id), ct);
        return File(content, mimeType, fileName);
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Documents, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(DocumentReadDto), 201)]
    public async Task<IActionResult> Upload([FromBody] UploadDocumentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UploadDocumentCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Documents, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(DocumentReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateDocumentCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Documents, Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteDocumentCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
