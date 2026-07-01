using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.UserTenants;
using DomuWave.Services.Extensions;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
namespace DomuWave.Microservice.Controllers;

[ApiExplorerSettings(IgnoreApi = false)]
[Route("api/user-tenants")]
public class UserTenantsController(
    ILogger<UserTenantsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Profilo dell'utente corrente nel tenant indicato (ruolo per-tenant).
    /// Il frontend lo usa al cambio tenant per ricalcolare profilo/menu/permessi.
    /// </summary>
    [HttpGet("profile-for-tenant/{tenantId:guid}")]
    [ProducesResponseType(typeof(DomuWave.Services.Dto.UserTenants.TenantProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfileForTenant(Guid tenantId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetTenantProfileCommand(CurrentUser.Id, tenantId), ct));

    /// <summary>Imposta il tenant indicato come predefinito per l'utente corrente.</summary>
    [HttpPatch("my-default/{tenantId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetMyDefaultTenant(Guid tenantId, CancellationToken ct)
    {
        var ok = await _mediator.GetResponse(new SetMyDefaultTenantCommand(CurrentUser.Id, tenantId), ct);
        if (!ok) return NotFound();
        return Ok(new { tenantId, isDefault = true });
    }

    [HttpGet("user/{userId:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<UserTenantReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId(long userId, CancellationToken ct)
    {
        var items = await _mediator.GetResponse(new GetUserTenantsByUserCommand(CurrentUser.Id, userId), ct);
        if (!items.Any()) return NotFound($"Nessun tenant trovato per l'utente {userId}.");
        return Ok(items.Select(k => k.ToDto()));
    }

    [HttpGet("tenant/{tenantId:guid}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<UserTenantReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByTenantId(Guid tenantId, CancellationToken ct)
    {
        var items = await _mediator.GetResponse(new GetUserTenantsByTenantCommand(CurrentUser.Id, tenantId), ct);
        return Ok(items.Select(k => k.ToDto()));
    }

    [HttpGet("user/{userId:long}/default")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefault(long userId, CancellationToken ct)
    {
        var item = await _mediator.GetResponse(new GetDefaultUserTenantCommand(CurrentUser.Id, userId), ct);
        if (item == null) return NotFound($"Nessun tenant di default configurato per l'utente {userId}.");
        return Ok(item.ToDto());
    }

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        UserTenant item = await _mediator.GetResponse(new GetUserTenantByIdCommand(CurrentUser.Id, id), ct);
        if (item == null) return NotFound($"UserTenant {id} non trovato.");
        return Ok(item.ToDto());
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] UserTenantCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _mediator.GetResponse(new CreateUserTenantCommand(CurrentUser.Id, dto.ToEntity()), ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UserTenantUpdateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var existing = await _mediator.GetResponse(new GetUserTenantByIdCommand(CurrentUser.Id, id), ct);
        if (existing == null) return NotFound($"UserTenant {id} non trovato.");
        dto.FillEntity(existing);
        var entity = await _mediator.GetResponse(new UpdateUserTenantCommand(CurrentUser.Id, id, existing), ct);
        return Ok(entity.ToDto());
    }

    [HttpPatch("user/{userId:long}/set-default")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefault(long userId, [FromBody] SetDefaultTenantDto dto, CancellationToken ct)
    {
        var entity = await _mediator.GetResponse(
            new SetDefaultUserTenantCommand(CurrentUser.Id, userId, dto.UserTenantId), ct);
        if (entity == null) return NotFound();
        return Ok(entity.ToDto());
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteUserTenantCommand(CurrentUser.Id, id), ct);
        return deleted ? NoContent() : NotFound($"UserTenant {id} non trovato.");
    }

    /// <summary>
    /// Restituisce tutti i condomini (raggruppati per tenant) ai quali l'utente è associato.
    /// Usato dalla pagina di gestione utenti per SuperAdmin.
    /// </summary>
    [HttpGet("user/{userId:long}/condominiums")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.UserTenants, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<UserCondominiumDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCondominiumsByUser(long userId, CancellationToken ct)
    {
        var items = await _mediator.GetResponse(new GetCondominiumsByUserCommand(CurrentUser.Id, userId), ct);
        return Ok(items);
    }
}
