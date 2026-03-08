using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using DomuWave.Application.Code;
using DomuWave.Services.Command.ChartOfAccounts;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Settings;

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
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ChartOfAccountsReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetChartOfAccountsByCondominiumCommand(CurrentUser.Id, condominiumId), ct);
        return Ok(result ?? new List<ChartOfAccountsReadDto>());
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ChartOfAccountsReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateChartOfAccountsDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new CreateChartOfAccountsCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetByCondominium), new { condominiumId = dto.CondominiumId }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ChartOfAccountsReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateChartOfAccountsDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new UpdateChartOfAccountsCommand(CurrentUser.Id, id, dto), ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.GetResponse(
            new DeleteChartOfAccountsCommand(CurrentUser.Id, id), ct);
        return NoContent();
    }

    [HttpPost("copy-from-condominium")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CopyFromCondominium(
        [FromBody] CopyChartOfAccountsDto dto, CancellationToken ct)
    {
        var copied = await _mediator.GetResponse(
            new CopyChartOfAccountsCommand(CurrentUser.Id, dto.SourceCondominiumId, dto.TargetCondominiumId), ct);
        return Ok(copied);
    }

    [HttpPost("apply-template/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyTemplate(int condominiumId, CancellationToken ct)
    {
        var created = await _mediator.GetResponse(
            new ApplyChartOfAccountsTemplateCommand(CurrentUser.Id, condominiumId), ct);
        return Ok(created);
    }
}
