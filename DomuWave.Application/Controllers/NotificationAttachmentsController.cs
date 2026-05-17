using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.NotificationAttachment;
using DomuWave.Services.Dto.NotificationAttachment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/notification-attachments")]
[Produces("application/json")]
public class NotificationAttachmentsController(
    ILogger<NotificationAttachmentsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-notification/{notificationId:long}")]
    [ProducesResponseType(typeof(IList<NotificationAttachmentReadDto>), 200)]
    public async Task<IActionResult> GetByNotification(long notificationId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetNotificationAttachmentsCommand(CurrentUser.Id, notificationId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(NotificationAttachmentReadDto), 201)]
    public async Task<IActionResult> Add([FromBody] AddAttachmentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new AddNotificationAttachmentCommand(CurrentUser.Id, dto.NotificationId, dto.DocumentId), ct);
        return StatusCode(201, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new RemoveNotificationAttachmentCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public class AddAttachmentDto
{
    public long NotificationId { get; set; }
    public int  DocumentId     { get; set; }
}
