using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using DomuWave.Application.Code;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Dto.Budget;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Settings;
using DomuWave.Services.Models;

namespace DomuWave.Microservice.Controllers;

[Route("api/chart-of-accounts")]
public class ChartOfAccountsController(
    ILogger<ChartOfAccountsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateAdminControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, "Budget", Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ChartOfAccountsReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetChartOfAccountsByCondominiumCommand(CurrentUser.Id, condominiumId), ct);
        return Ok(result ?? new List<ChartOfAccountsReadDto>());
    }
}
