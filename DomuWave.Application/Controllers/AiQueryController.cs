using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.AI;
using DomuWave.Services.AI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

[Route("api/ai")]
[Produces("application/json")]
public class AiQueryController(
    ILogger<AiQueryController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IAiOrchestratorService orchestrator)
    : TenantContextController(logger, configuration)
{
    private readonly IAiOrchestratorService _orchestrator = orchestrator;

    /// <summary>Esegue una query in linguaggio naturale sui dati condominiali.</summary>
    [HttpPost("query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AiQueryResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Query([FromBody] AiQueryRequest request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("La domanda è obbligatoria.");

        // Il tenant è derivato dall'header X-Tenant-Id, non dal body (sicurezza).
        request.TenantId = TenantId.GetValueOrDefault();

        var result = await _orchestrator.HandleQueryAsync(request, CurrentUser, ct);
        return Ok(result);
    }
}
