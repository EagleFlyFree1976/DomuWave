using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.ChartOfAccountsCategoryTemplate;
using DomuWave.Services.Dto.ChartOfAccountsCategoryTemplate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione dei template di categoria del piano dei conti.
/// Accessibile solo ai SuperAdmin; i template vengono applicati automaticamente
/// alla creazione di ogni nuovo tenant.
/// </summary>
[Route("api/chart-of-accounts-category-templates")]
[Produces("application/json")]
public class ChartOfAccountsCategoryTemplateController(
    ILogger<ChartOfAccountsCategoryTemplateController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateAdminControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.ChartOfAccountsCategory, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ChartOfAccountsCategoryTemplateReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetAllChartOfAccountsCategoryTemplatesCommand(CurrentUser.Id), ct));

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.ChartOfAccountsCategory, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ChartOfAccountsCategoryTemplateReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateChartOfAccountsCategoryTemplateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateChartOfAccountsCategoryTemplateCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ChartOfAccountsCategory, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ChartOfAccountsCategoryTemplateReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateChartOfAccountsCategoryTemplateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateChartOfAccountsCategoryTemplateCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.ChartOfAccountsCategory, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.GetResponse(new DeleteChartOfAccountsCategoryTemplateCommand(CurrentUser.Id, id), ct);
        return NoContent();
    }
}
