using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.ExtraordinaryWork;
using DomuWave.Services.Dto.ExtraordinaryWork;
using Microsoft.AspNetCore.Mvc;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/extraordinary-works")]
[Produces("application/json")]
public class ExtraordinaryWorksController(
    ILogger<ExtraordinaryWorksController> logger,
    Microsoft.Extensions.Options.IOptionsMonitor<CPQ.Core.Settings.OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ExtraordinaryWorkReadDto>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetWorksByCondominiumCommand(CurrentUser.Id, condominiumId, TenantId.GetValueOrDefault()), ct));

    [HttpGet("condominium/{condominiumId:int}/status/{status}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ExtraordinaryWorkReadDto>))]
    public async Task<IActionResult> GetByStatus(int condominiumId, string status, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetWorksByStatusCommand(CurrentUser.Id, condominiumId, status), ct));

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExtraordinaryWorkReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetWorkByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ExtraordinaryWorkReadDto))]
    public async Task<IActionResult> Create([FromBody] CreateExtraordinaryWorkDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateWorkCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExtraordinaryWorkReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExtraordinaryWorkDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateWorkCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteWorkCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // ── Quotes sub-resource ──────────────────────────────────────────

    [HttpGet("{workId:int}/quotes")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<WorkQuoteReadDto>))]
    public async Task<IActionResult> GetQuotes(int workId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetQuotesByWorkCommand(CurrentUser.Id, workId), ct));

    [HttpPost("{workId:int}/quotes")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkQuoteReadDto))]
    public async Task<IActionResult> CreateQuote(int workId, [FromBody] CreateWorkQuoteDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        dto.WorkId = workId;
        var result = await _mediator.GetResponse(new CreateQuoteCommand(CurrentUser.Id, dto), ct);
        return Created($"api/extraordinary-works/{workId}/quotes/{result.Id}", result);
    }

    [HttpPut("{workId:int}/quotes/{quoteId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkQuoteReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuote(int workId, int quoteId, [FromBody] UpdateWorkQuoteDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateQuoteCommand(CurrentUser.Id, quoteId, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{workId:int}/quotes/{quoteId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteQuote(int workId, int quoteId, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteQuoteCommand(CurrentUser.Id, quoteId), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("{workId:int}/quotes/{quoteId:int}/approve")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkQuoteReadDto))]
    public async Task<IActionResult> ApproveQuote(int workId, int quoteId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new ApproveQuoteCommand(CurrentUser.Id, quoteId), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{workId:int}/quotes/{quoteId:int}/reject")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkQuoteReadDto))]
    public async Task<IActionResult> RejectQuote(int workId, int quoteId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new RejectQuoteCommand(CurrentUser.Id, quoteId), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // ── Documents sub-resource ───────────────────────────────────────

    [HttpPost("{workId:int}/quotes/{quoteId:int}/documents")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkQuoteDocumentReadDto))]
    public async Task<IActionResult> AddDocument(int workId, int quoteId, [FromBody] CreateWorkQuoteDocumentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        dto.QuoteId = quoteId;
        var result = await _mediator.GetResponse(new AddQuoteDocumentCommand(CurrentUser.Id, dto), ct);
        return Created($"api/extraordinary-works/{workId}/quotes/{quoteId}/documents/{result.Id}", result);
    }

    [HttpDelete("{workId:int}/quotes/{quoteId:int}/documents/{documentId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.ExtraordinaryWorks, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDocument(int workId, int quoteId, int documentId, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteQuoteDocumentCommand(CurrentUser.Id, documentId), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
