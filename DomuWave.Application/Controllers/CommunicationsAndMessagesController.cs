using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
 
namespace DomuWave.Microservice.Controllers;

[Route("api/communications")]
public class CommunicationsController(
    ILogger<CommunicationsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetAllCommunicationsCommand(CurrentUser.Id), ct));

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetCommunicationByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetCommunicationsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("condominium/{condominiumId:int}/visible")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetVisible(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetVisibleCommunicationsCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("condominium/{condominiumId:int}/unread/{userId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetUnread(int condominiumId, long userId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetUnreadCommunicationsCommand(CurrentUser.Id, condominiumId, userId), ct));

    [HttpGet("condominium/{condominiumId:int}/type/{type}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetByType(int condominiumId, string type, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetCommunicationsByTypeCommand(CurrentUser.Id, condominiumId, type), ct));

    [HttpGet("condominium/{condominiumId:int}/priority/{priority}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetByPriority(int condominiumId, string priority, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetCommunicationsByPriorityCommand(CurrentUser.Id, condominiumId, priority), ct));

    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> Create([FromBody] CreateCommunicationRequest request, CancellationToken ct)
    {
        if (request == null) return BadRequest("Il corpo della richiesta è obbligatorio.");
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Title è obbligatorio.");
        if (string.IsNullOrWhiteSpace(request.CommunicationType)) return BadRequest("CommunicationType è obbligatorio.");

        var entity = new Communication
        {
            Title = request.Title, Content = request.Content,
            CommunicationType = request.CommunicationType, Priority = request.Priority,
            PublicationDate = request.PublicationDate, ExpirationDate = request.ExpirationDate,
            SendEmail = request.SendEmail, IsVisible = request.IsVisible,
            AttachmentPath = request.AttachmentPath
        };

        var created = await _mediator.GetResponse(new CreateCommunicationCommand(CurrentUser.Id, entity), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommunicationRequest request, CancellationToken ct)
    {
        if (request == null) return BadRequest("Il corpo della richiesta è obbligatorio.");

        var entity = new Communication
        {
            Title = request.Title, Content = request.Content,
            CommunicationType = request.CommunicationType, Priority = request.Priority,
            PublicationDate = request.PublicationDate, ExpirationDate = request.ExpirationDate,
            SendEmail = request.SendEmail, IsVisible = request.IsVisible,
            AttachmentPath = request.AttachmentPath
        };

        var result = await _mediator.GetResponse(new UpdateCommunicationCommand(CurrentUser.Id, id, entity), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:int}/publish")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new PublishCommunicationCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Authorizations, Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new DeleteCommunicationCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }
}

public class CreateCommunicationRequest
{
    public int CondominiumId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string CommunicationType { get; set; }
    public string Priority { get; set; }
    public DateTime PublicationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool SendEmail { get; set; }
    public bool IsVisible { get; set; }
    public string AttachmentPath { get; set; }
}

public class UpdateCommunicationRequest
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string CommunicationType { get; set; }
    public string Priority { get; set; }
    public DateTime PublicationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool SendEmail { get; set; }
    public bool IsVisible { get; set; }
    public string AttachmentPath { get; set; }
}
