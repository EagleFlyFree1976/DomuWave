using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.Dashboard;
using DomuWave.Services.Dto.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController(
    ILogger<DashboardController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("summary")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Dashboard, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardSummaryDto))]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetDashboardSummaryCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct);
        return Ok(result);
    }
}
