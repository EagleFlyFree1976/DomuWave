using CPQ.Core.Controllers;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.TenantDisplay;
using DomuWave.Services.Dto.TenantDisplay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/tenant-display")]
[Produces("application/json")]
public class TenantDisplayController(
    ILogger<TenantDisplayController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    /// <summary>Impostazioni di visualizzazione del tenant corrente (sempre disponibili, default SoloColore).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(TenantDisplaySettingsReadDto), 200)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetTenantDisplaySettingsCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct);
        return Ok(result);
    }

    /// <summary>Aggiorna le impostazioni di visualizzazione del tenant corrente.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(TenantDisplaySettingsReadDto), 200)]
    public async Task<IActionResult> Upsert([FromBody] UpsertTenantDisplaySettingsDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpsertTenantDisplaySettingsCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), dto), ct);
        return Ok(result);
    }
}
