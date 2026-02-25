using CPQ.Core.Exceptions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Dto.UserTenants;
using DomuWave.Services.Extensions;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers.DomuWave.Application.Controllers;

/// <summary>
/// Gestione delle associazioni utente-tenant.
/// Permette di assegnare tenant agli utenti e configurarne uno come default.
/// </summary>
[ApiExplorerSettings(IgnoreApi = false)]
[Route("api/[controller]")]
public class UserTenantsController : PrivateControllerBase
{
    private readonly IUserTenantService _userTenantService;

    public UserTenantsController(
        ILogger<UserTenantsController> logger,
        IOptionsMonitor<OxCoreSettings> configuration,
        IUserTenantService userTenantService)
        : base(logger, configuration)
    {
        _userTenantService = userTenantService;
    }

    // ── GET /api/usertenants/user/{userId} ────────────────────────────────────

    /// <summary>Restituisce tutti i tenant assegnati a un utente.</summary>
    [HttpGet("user/{userId:long}")]
    [ProducesResponseType(typeof(IList<UserTenantReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUserId(
        long userId, CancellationToken ct)
    {
        var items = await _userTenantService.GetByUserIdAsync(userId, CurrentUser, ct);

        if (!items.Any())
            return NotFound($"Nessun tenant trovato per l'utente {userId}.");

        return Ok(items.Select(MapToDto));
    }

    // ── GET /api/usertenants/user/{userId}/default ────────────────────────────

    /// <summary>Restituisce il tenant di default per un utente.</summary>
    [HttpGet("user/{userId:long}/default")]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDefault(
        long userId, CancellationToken ct)
    {
        var item = await _userTenantService.GetDefaultByUserIdAsync(userId, CurrentUser, ct);

        if (item == null)
            return NotFound($"Nessun tenant di default configurato per l'utente {userId}.");

        return Ok(MapToDto(item));
    }

    // ── GET /api/usertenants/{id} ─────────────────────────────────────────────

    /// <summary>Restituisce una singola associazione utente-tenant per ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await _userTenantService.GetByIdAsync(id, CurrentUser, ct);

        if (item == null)
            return NotFound($"UserTenant {id} non trovato.");

        return Ok(MapToDto(item));
    }

    // ── POST /api/usertenants ─────────────────────────────────────────────────

    /// <summary>Assegna un tenant a un utente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] UserTenantCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var entity = await _userTenantService.CreateAsync(dto.ToEntity(), CurrentUser, ct);


            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, MapToDto(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ── PUT /api/usertenants/{id} ─────────────────────────────────────────────

    /// <summary>Aggiorna IsDefault e/o IsActive di un'associazione.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UserTenantUpdateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var exists = await _userTenantService.GetByIdAsync(id, CurrentUser, ct);
            if (exists == null)
                throw new NotFoundException("Entita non trovata");

            dto.FillEntity(exists);

                var entity = await _userTenantService.UpdateAsync(exists, CurrentUser, ct);
            return Ok(MapToDto(entity));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ── PATCH /api/usertenants/user/{userId}/set-default ─────────────────────

    /// <summary>
    /// Imposta il tenant di default per un utente.
    /// Rimuove automaticamente il flag dagli altri tenant dell'utente.
    /// </summary>
    [HttpPatch("user/{userId:long}/set-default")]
    [ProducesResponseType(typeof(UserTenantReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(
        long userId, [FromBody] SetDefaultTenantDto dto, CancellationToken ct)
    {
        try
        {
            var entity = await _userTenantService.SetDefaultAsync(userId, dto.UserTenantId, CurrentUser, ct);
            return Ok(MapToDto(entity));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ── DELETE /api/usertenants/{id} ──────────────────────────────────────────

    /// <summary>
    /// Rimuove (soft delete) un'associazione utente-tenant.
    /// Non è possibile eliminare il tenant di default.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _userTenantService.DeleteAsync(id, CurrentUser, ct);
            return deleted ? NoContent() : NotFound($"UserTenant {id} non trovato.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ── Mapper privato ────────────────────────────────────────────────────────

    private static UserTenantReadDto MapToDto(UserTenant x) => new()
    {
        UserTenantId = x.Id,
        UserId = x.UserId,
        TenantId = x.Tenant.Id,
        TenantName = x.Tenant.Name,
        TenantCode = x.Tenant.Code,
        IsDefault = x.IsDefault,
        IsActive = x.IsActive,
    };
}