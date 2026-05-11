using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Services;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Communication;
using DomuWave.Services.Dto.Communication;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;


namespace DomuWave.Microservice.Controllers;

[Route("api/communications")]
[Produces("application/json")]
public class CommunicationsController(
    ILogger<CommunicationsController> logger,
    IOptionsMonitor<OxCoreSettings>   configuration,
    IMediator                         mediator,
    ICoreAuthorizationManager         authorizationManager)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator                 _mediator             = mediator;
    private readonly ICoreAuthorizationManager _authorizationManager = authorizationManager;

    [HttpGet("permissions")]
    [ProducesResponseType(typeof(CanAuthorization), 200)]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        var perms = await _authorizationManager.UserIsAuthorizedFor(
            CurrentUser, AuthorizationKeys.Communications, Modules.DomuWaveModule, ct);
        return Ok(perms);
    }

    [HttpGet("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(IList<CommunicationReadDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetAllCommunicationsCommand(CurrentUser.Id), ct));

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(CommunicationReadDto), 200)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetCommunicationByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(IList<CommunicationReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, [FromQuery] bool archived = false, CancellationToken ct = default)
        => Ok(await _mediator.GetResponse(new GetCommunicationsByCondominiumCommand(CurrentUser.Id, condominiumId, archived), ct));

    [HttpGet("condominium/{condominiumId:int}/visible")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(IList<CommunicationReadDto>), 200)]
    public async Task<IActionResult> GetVisible(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetVisibleCommunicationsCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("condominium/{condominiumId:int}/unread/{userId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(IList<CommunicationReadDto>), 200)]
    public async Task<IActionResult> GetUnread(int condominiumId, long userId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetUnreadCommunicationsCommand(CurrentUser.Id, condominiumId, userId), ct));

    [HttpGet("condominium/{condominiumId:int}/type/{type}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(IList<CommunicationReadDto>), 200)]
    public async Task<IActionResult> GetByType(int condominiumId, string type, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetCommunicationsByTypeCommand(CurrentUser.Id, condominiumId, type), ct));

    [HttpGet("condominium/{condominiumId:int}/priority/{priority}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(IList<CommunicationReadDto>), 200)]
    public async Task<IActionResult> GetByPriority(int condominiumId, string priority, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetCommunicationsByPriorityCommand(CurrentUser.Id, condominiumId, priority), ct));

    [HttpPost("")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(CommunicationReadDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateCommunicationDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateCommunicationCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    [ProducesResponseType(typeof(CommunicationReadDto), 200)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommunicationDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateCommunicationCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:int}/publish")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    public async Task<IActionResult> Publish(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new PublishCommunicationCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return Ok();
    }

    [HttpPost("{id:int}/archive")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    public async Task<IActionResult> Archive(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new ArchiveCommunicationCommand(CurrentUser.Id, id, true), ct);
        if (!result) return NotFound();
        return Ok();
    }

    [HttpPost("{id:int}/unarchive")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    public async Task<IActionResult> Unarchive(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new ArchiveCommunicationCommand(CurrentUser.Id, id, false), ct);
        if (!result) return NotFound();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Authorizations, Modules.AuthModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new DeleteCommunicationCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }
}
