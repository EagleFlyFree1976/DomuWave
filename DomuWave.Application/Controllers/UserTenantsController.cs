using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.UserTenant;
using DomuWave.Services.Dto.UserTenants;
using DomuWave.Services.Extensions;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Extensions;
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

    [HttpGet("user/{userId:long}")]
    [ProducesResponseType(typeof(IList<UserTenantReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUserId(long userId, CancellationToken ct)
    {
        var items = await _mediator.GetResponse(new GetUserTenantsByUserCommand(CurrentUser.Id, userId), ct);
        if (!items.Any()) return NotFound($"Nessun tenant trovato per l'utente {userId}.");
        return Ok(items.Select(MapToDto));
    }

    [HttpGet("user/{userId:long}/default")]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefault(long userId, CancellationToken ct)
    {
        var item = await _mediator.GetResponse(new GetDefaultUserTenantCommand(CurrentUser.Id, userId), ct);
        if (item == null) return NotFound($"Nessun tenant di default configurato per l'utente {userId}.");
        return Ok(MapToDto(item));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _mediator.GetResponse(new GetUserTenantByIdCommand(CurrentUser.Id, id), ct);
        if (item == null) return NotFound($"UserTenant {id} non trovato.");
        return Ok(MapToDto(item));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] UserTenantCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _mediator.GetResponse(new CreateUserTenantCommand(CurrentUser.Id, dto.ToEntity()), ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, MapToDto(entity));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UserTenantUpdateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var existing = await _mediator.GetResponse(new GetUserTenantByIdCommand(CurrentUser.Id, id), ct);
        if (existing == null) return NotFound($"UserTenant {id} non trovato.");
        dto.FillEntity(existing);
        var entity = await _mediator.GetResponse(new UpdateUserTenantCommand(CurrentUser.Id, id, existing), ct);
        return Ok(MapToDto(entity));
    }

    [HttpPatch("user/{userId:long}/set-default")]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefault(long userId, [FromBody] SetDefaultTenantDto dto, CancellationToken ct)
    {
        var entity = await _mediator.GetResponse(
            new SetDefaultUserTenantCommand(CurrentUser.Id, userId, dto.UserTenantId), ct);
        if (entity == null) return NotFound();
        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteUserTenantCommand(CurrentUser.Id, id), ct);
        return deleted ? NoContent() : NotFound($"UserTenant {id} non trovato.");
    }

    private static UserTenantReadDto MapToDto(UserTenant x) => new()
    {
        UserTenantId = x.Id,
        UserId       = x.UserId,
        TenantId     = x.Tenant.Id,
        TenantName   = x.Tenant.Name,
        TenantCode   = x.Tenant.Code,
        IsDefault    = x.IsDefault,
        IsActive     = x.IsActive,
    };
}
