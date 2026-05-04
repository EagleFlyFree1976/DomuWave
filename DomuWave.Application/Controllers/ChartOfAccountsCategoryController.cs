using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.ChartOfAccountsCategory;
using DomuWave.Services.Dto.ChartOfAccountsCategory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

[Route("api/chart-of-accounts-categories")]
[Produces("application/json")]
public class ChartOfAccountsCategoryController(
    ILogger<ChartOfAccountsCategoryController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    [HttpGet]
    [ProducesResponseType(typeof(IList<ChartOfAccountsCategoryReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetChartOfAccountsCategoriesCommand(CurrentUser.Id, TenantId.GetValueOrDefault()), ct));

    [HttpPost]
    [ProducesResponseType(typeof(ChartOfAccountsCategoryReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateChartOfAccountsCategoryDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateChartOfAccountsCategoryCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), dto), ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ChartOfAccountsCategoryReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateChartOfAccountsCategoryDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateChartOfAccountsCategoryCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.GetResponse(new DeleteChartOfAccountsCategoryCommand(CurrentUser.Id, id), ct);
        return NoContent();
    }

    [HttpPost("import-from-template")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportFromTemplate([FromBody] IList<int> templateIds, CancellationToken ct)
    {
        var imported = await _mediator.GetResponse(
            new ImportCategoriesFromTemplateCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), templateIds), ct);
        return Ok(imported);
    }
}
