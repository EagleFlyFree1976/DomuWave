using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.ChartOfAccountsTemplate;
using DomuWave.Services.Dto.ChartOfAccountsTemplate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/chart-of-accounts-templates")]
[Produces("application/json")]
public class ChartOfAccountsTemplatesController(
    ILogger<ChartOfAccountsTemplatesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    // ── Templates ─────────────────────────────────────────────────────────────

    [HttpGet]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ChartOfAccountsTemplateReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetChartOfAccountsTemplatesCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct));

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ChartOfAccountsTemplateReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateChartOfAccountsTemplateDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new CreateChartOfAccountsTemplateCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), dto), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ChartOfAccountsTemplateReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateChartOfAccountsTemplateDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new UpdateChartOfAccountsTemplateCommand(CurrentUser.Id, id, dto), ct);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.GetResponse(new DeleteChartOfAccountsTemplateCommand(CurrentUser.Id, id), ct);
        return NoContent();
    }

    // ── Template Items ────────────────────────────────────────────────────────

    [HttpGet("{templateId:int}/items")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<ChartOfAccountsTemplateItemReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetItems(int templateId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetChartOfAccountsTemplateItemsCommand(CurrentUser.Id, templateId), ct));

    [HttpPost("{templateId:int}/items")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ChartOfAccountsTemplateItemReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateItem(int templateId, [FromBody] SaveChartOfAccountsTemplateItemDto dto, CancellationToken ct)
    {
        dto.TemplateId = templateId;
        var result = await _mediator.GetResponse(
            new SaveChartOfAccountsTemplateItemCommand(CurrentUser.Id, null, dto), ct);
        return CreatedAtAction(nameof(GetItems), new { templateId }, result);
    }

    [HttpPut("{templateId:int}/items/{itemId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ChartOfAccountsTemplateItemReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateItem(int templateId, int itemId, [FromBody] SaveChartOfAccountsTemplateItemDto dto, CancellationToken ct)
    {
        dto.TemplateId = templateId;
        var result = await _mediator.GetResponse(
            new SaveChartOfAccountsTemplateItemCommand(CurrentUser.Id, itemId, dto), ct);
        return Ok(result);
    }

    [HttpDelete("{templateId:int}/items/{itemId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteItem(int templateId, int itemId, CancellationToken ct)
    {
        await _mediator.GetResponse(new DeleteChartOfAccountsTemplateItemCommand(CurrentUser.Id, itemId), ct);
        return NoContent();
    }

    // ── Set default ───────────────────────────────────────────────────────────

    [HttpPatch("{id:int}/set-default")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
    {
        await _mediator.GetResponse(new SetDefaultChartOfAccountsTemplateCommand(CurrentUser.Id, id), ct);
        return NoContent();
    }

    // ── Apply ─────────────────────────────────────────────────────────────────

    [HttpPost("{templateId:int}/apply/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.ChartOfAccountsTemplate, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> Apply(int templateId, int condominiumId, CancellationToken ct)
    {
        var created = await _mediator.GetResponse(
            new ApplyChartOfAccountsTemplateToCondominiumCommand(CurrentUser.Id, templateId, condominiumId), ct);
        return Ok(created);
    }
}
