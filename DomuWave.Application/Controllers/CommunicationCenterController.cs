using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.BoardPost;
using DomuWave.Services.Command.Fault;
using DomuWave.Services.Command.PrivateThread;
using DomuWave.Services.Dto.BoardPost;
using DomuWave.Services.Dto.Fault;
using DomuWave.Services.Dto.PrivateThread;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

// ── Bacheca ──────────────────────────────────────────────────────────────────

[Route("api/board-posts")]
[Produces("application/json")]
public class BoardPostsController(
    ILogger<BoardPostsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(typeof(IList<BoardPostReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetBoardPostsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(BoardPostReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateBoardPostDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateBoardPostCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BoardPostReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBoardPostDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateBoardPostCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteBoardPostCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("{postId:int}/comments")]
    [ProducesResponseType(typeof(IList<BoardPostCommentReadDto>), 200)]
    public async Task<IActionResult> GetComments(int postId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetBoardPostCommentsByPostCommand(CurrentUser.Id, postId), ct));

    [HttpPost("{postId:int}/comments")]
    [ProducesResponseType(typeof(BoardPostCommentReadDto), 201)]
    public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateBoardPostCommentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        dto.BoardPostId = postId;
        var result = await _mediator.GetResponse(new CreateBoardPostCommentCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpDelete("comments/{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteComment(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteBoardPostCommentCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

// ── Guasti ───────────────────────────────────────────────────────────────────

[Route("api/faults")]
[Produces("application/json")]
public class FaultsController(
    ILogger<FaultsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(typeof(IList<FaultReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetFaultsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FaultReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetFaultByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FaultReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateFaultDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateFaultCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(FaultReadDto), 200)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateFaultStatusDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new UpdateFaultStatusCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteFaultCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("{faultId:int}/messages")]
    [ProducesResponseType(typeof(IList<FaultMessageReadDto>), 200)]
    public async Task<IActionResult> GetMessages(int faultId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetFaultMessagesByFaultCommand(CurrentUser.Id, faultId), ct));

    [HttpPost("{faultId:int}/messages")]
    [ProducesResponseType(typeof(FaultMessageReadDto), 201)]
    public async Task<IActionResult> CreateMessage(int faultId, [FromBody] CreateFaultMessageDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        dto.FaultId = faultId;
        var result = await _mediator.GetResponse(new CreateFaultMessageCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }
}

// ── Messaggi Privati ─────────────────────────────────────────────────────────

[Route("api/private-threads")]
[Produces("application/json")]
public class PrivateThreadsController(
    ILogger<PrivateThreadsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(typeof(IList<PrivateThreadReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetPrivateThreadsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpPost("get-or-create")]
    [ProducesResponseType(typeof(PrivateThreadReadDto), 200)]
    public async Task<IActionResult> GetOrCreate([FromBody] GetOrCreateThreadDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetOrCreatePrivateThreadCommand(CurrentUser.Id, dto.CondominiumId, dto.CondominioUserId), ct);
        return Ok(result);
    }

    [HttpGet("{threadId:int}/messages")]
    [ProducesResponseType(typeof(IList<PrivateMessageReadDto>), 200)]
    public async Task<IActionResult> GetMessages(int threadId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetPrivateMessagesByThreadCommand(CurrentUser.Id, threadId), ct));

    [HttpPost("{threadId:int}/messages")]
    [ProducesResponseType(typeof(PrivateMessageReadDto), 201)]
    public async Task<IActionResult> CreateMessage(int threadId, [FromBody] CreatePrivateMessageDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        dto.ThreadId = threadId;
        var result = await _mediator.GetResponse(new CreatePrivateMessageCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPost("{threadId:int}/mark-read")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkRead(int threadId, CancellationToken ct)
    {
        await _mediator.GetResponse(new MarkThreadMessagesReadCommand(CurrentUser.Id, threadId, (long)CurrentUser.Id), ct);
        return NoContent();
    }
}

// ── DTO locale per GetOrCreate ───────────────────────────────────────────────
public class GetOrCreateThreadDto
{
    public int  CondominiumId    { get; set; }
    public long CondominioUserId { get; set; }
}
