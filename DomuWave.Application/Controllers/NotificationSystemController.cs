using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.CommunicationNotification;
using DomuWave.Services.Command.NotificationTemplate;
using DomuWave.Services.Command.TenantSmtp;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Dto.CommunicationNotification;
using DomuWave.Services.Dto.NotificationTemplate;
using DomuWave.Services.Dto.TenantSmtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

// ─── NotificationTemplates ────────────────────────────────────────────────────

[Route("api/notification-templates")]
[Produces("application/json")]
public class NotificationTemplatesController(
    ILogger<NotificationTemplatesController> logger,
    IOptionsMonitor<OxCoreSettings>          configuration,
    IMediator                                mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(typeof(IList<NotificationTemplateReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetNotificationTemplatesByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(NotificationTemplateReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetNotificationTemplateByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(NotificationTemplateReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateNotificationTemplateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateNotificationTemplateCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(NotificationTemplateReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationTemplateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateNotificationTemplateCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteNotificationTemplateCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("seed-defaults/{condominiumId:int}")]
    [ProducesResponseType(typeof(IList<NotificationTemplateReadDto>), 201)]
    public async Task<IActionResult> SeedDefaults(int condominiumId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new SeedDefaultNotificationTemplatesCommand(CurrentUser.Id, condominiumId), ct);
        return StatusCode(201, result);
    }
}

// ─── TenantSmtp ───────────────────────────────────────────────────────────────

[Route("api/tenant-smtp")]
[Produces("application/json")]
public class TenantSmtpController(
    ILogger<TenantSmtpController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    

    [HttpGet]
    [ProducesResponseType(typeof(TenantSmtpSettingsReadDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetTenantSmtpSettingsCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(TenantSmtpSettingsReadDto), 200)]
    public async Task<IActionResult> Upsert([FromBody] UpsertTenantSmtpSettingsDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpsertTenantSmtpSettingsCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), dto), ct);
        return Ok(result);
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test([FromBody] TestSmtpRequest request, CancellationToken ct)
    {
        await _mediator.GetResponse(new TestTenantSmtpCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), request.EmailTo), ct);
        return Ok(new { sent = true });
    }
}

public record TestSmtpRequest(string EmailTo);
public record RegenerateTextsRequest(int? NotificationTemplateId);

// ─── CommunicationNotifications ──────────────────────────────────────────────

[Route("api/communication-notifications")]
[Produces("application/json")]
public class CommunicationNotificationsController(
    ILogger<CommunicationNotificationsController> logger,
    IOptionsMonitor<OxCoreSettings>               configuration,
    IMediator                                     mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-communication/{communicationId:int}")]
    [ProducesResponseType(typeof(IList<CommunicationNotificationReadDto>), 200)]
    public async Task<IActionResult> GetByCommunication(int communicationId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetNotificationsByCommunicationCommand(CurrentUser.Id, communicationId), ct));

    [HttpPost("generate/{communicationId:int}")]
    [ProducesResponseType(typeof(IList<CommunicationNotificationReadDto>), 201)]
    public async Task<IActionResult> Generate(int communicationId, [FromBody] GenerateNotificationsDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GenerateCommunicationNotificationsCommand(CurrentUser.Id, communicationId, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPost("generate-from-fees")]
    [ProducesResponseType(typeof(GenerateFromFeesResultDto), 201)]
    public async Task<IActionResult> GenerateFromFees([FromBody] GenerateNotificationsFromFeesDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new GenerateNotificationsFromFeesCommand(CurrentUser.Id, dto), ct);
        return StatusCode(201, result);
    }

    [HttpPost("send-email/{communicationId:int}")]
    [ProducesResponseType(typeof(SendNotificationsResultDto), 200)]
    public async Task<IActionResult> SendEmail(int communicationId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new SendEmailNotificationsCommand(CurrentUser.Id, communicationId), ct));

    [HttpPost("{id:long}/send-email")]
    [ProducesResponseType(typeof(CommunicationNotificationReadDto), 200)]
    public async Task<IActionResult> SendSingleEmail(long id, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new SendSingleEmailNotificationCommand(CurrentUser.Id, id), ct));

    [HttpPost("{id:long}/mark-printed")]
    [ProducesResponseType(typeof(CommunicationNotificationReadDto), 200)]
    public async Task<IActionResult> MarkPrinted(long id, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new MarkNotificationPrintedCommand(CurrentUser.Id, id), ct));

    [HttpPost("{id:long}/mark-sent")]
    [ProducesResponseType(typeof(CommunicationNotificationReadDto), 200)]
    public async Task<IActionResult> MarkSent(long id, [FromBody] UpdateNotificationStatusDto dto, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new MarkNotificationSentCommand(CurrentUser.Id, id, dto), ct));

    [HttpPost("{id:long}/mark-delivered")]
    [ProducesResponseType(typeof(CommunicationNotificationReadDto), 200)]
    public async Task<IActionResult> MarkDelivered(long id, [FromBody] UpdateNotificationStatusDto dto, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new MarkNotificationDeliveredCommand(CurrentUser.Id, id, dto), ct));

    [HttpGet("{id:long}/attachment-pdf")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> GetAttachmentPdf(long id, CancellationToken ct)
    {
        var bytes = await _mediator.GetResponse(new GetNotificationAttachmentPdfCommand(CurrentUser.Id, id), ct);
        return File(bytes, "application/pdf", $"cedolini-{id}.pdf");
    }

    [HttpGet("batch-pdf/{communicationId:int}")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> GetBatchPdf(int communicationId, CancellationToken ct)
    {
        var bytes = await _mediator.GetResponse(new GetNotificationBatchPdfCommand(CurrentUser.Id, communicationId), ct);
        return File(bytes, "application/pdf", $"comunicazioni-{communicationId}.pdf");
    }

    [HttpPost("regenerate-texts/{communicationId:int}")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<IActionResult> RegenerateTexts(int communicationId, [FromBody] RegenerateTextsRequest request, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new RegenerateNotificationTextsCommand(CurrentUser.Id, communicationId, request.NotificationTemplateId), ct));

    [HttpPut("{id:long}/text")]
    [ProducesResponseType(typeof(CommunicationNotificationReadDto), 200)]
    public async Task<IActionResult> UpdateText(long id, [FromBody] UpdateNotificationTextDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateNotificationTextCommand(CurrentUser.Id, id, dto), ct);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteCommunicationNotificationCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
