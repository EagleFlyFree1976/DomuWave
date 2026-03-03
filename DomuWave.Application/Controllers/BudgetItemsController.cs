using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using DomuWave.Application.Code;
using DomuWave.Services.Command.BudgetItem;
using DomuWave.Services.Dto.Budget;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Settings;
using DomuWave.Services.Models;

namespace DomuWave.Microservice.Controllers;

[Route("api/budget-items")]
public class BudgetItemsController(
    ILogger<BudgetItemsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateAdminControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-budget/{budgetId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, "Budget", Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<BudgetItemReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBudget(int budgetId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetBudgetItemsByBudgetCommand(CurrentUser.Id, budgetId), ct);
        return Ok(result ?? new List<BudgetItemReadDto>());
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, "Budget", Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BudgetItemReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetItemDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new CreateBudgetItemCommand(CurrentUser.Id, dto), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, "Budget", Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BudgetItemReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBudgetItemDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateBudgetItemCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, "Budget", Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new DeleteBudgetItemCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }
}
