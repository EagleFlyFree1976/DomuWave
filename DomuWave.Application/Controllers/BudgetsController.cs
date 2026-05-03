using CPQ.Core.ActionFilters;
using CPQ.Core.Extensions;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Budget;
using DomuWave.Services.Dto.Budget;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Settings;
using DomuWave.Services.Models;

namespace DomuWave.Microservice.Controllers;

[Route("api/budgets")]
public class BudgetsController(
    ILogger<BudgetsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateAdminControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    private Guid TenantGuid => Guid.Parse(HttpContext.Items["TenantId"]?.ToString() ?? Guid.Empty.ToString());

    [HttpGet("by-condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<BudgetReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetBudgetsByCondominiumCommand(CurrentUser.Id, condominiumId, TenantGuid), ct);
        return Ok(result ?? new List<BudgetReadDto>());
    }

    [HttpGet("by-fiscal-year/{fiscalYearId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<BudgetReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByFiscalYear(int fiscalYearId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetBudgetsByFiscalYearCommand(CurrentUser.Id, fiscalYearId, TenantGuid), ct);
        return Ok(result ?? new List<BudgetReadDto>());
    }

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BudgetReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetBudgetByIdCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BudgetReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new CreateBudgetCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BudgetReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBudgetDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateBudgetCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:int}/pre-approve")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    public async Task<IActionResult> PreApprove(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new PreApproveBudgetCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveInstallmentOptionsDto options, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new ApproveBudgetCommand(CurrentUser.Id, id)
            {
                NumberOfInstallments = options?.NumberOfInstallments ?? 4,
                FirstDueDate         = options?.FirstDueDate ?? DateTime.Today,
            }, ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/reopen")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    public async Task<IActionResult> Reopen(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new ReopenBudgetCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/recalculate-items")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    public async Task<IActionResult> RecalculateItems(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new RecalculateBudgetItemsCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/fix-orphan-items")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> FixOrphanItems(int id, CancellationToken ct)
    {
        var created = await _mediator.GetResponse(
            new FixOrphanBudgetItemsCommand(CurrentUser.Id, id), ct);
        return Ok(created);
    }

    [HttpPost("{id:int}/close")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new CloseBudgetCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/generate-installments")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    public async Task<IActionResult> GenerateInstallments(
        int id, [FromBody] GenerateInstallmentsFromBudgetRequest request, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GenerateInstallmentsFromBudgetCommand(
                CurrentUser.Id, id,
                request?.NumberOfInstallments ?? 4,
                request?.FirstDueDate ?? DateTime.Today,
                request?.Force ?? false), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/regenerate-allocations")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(RegenerateExpenseAllocationsResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> RegenerateAllocations(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new RegenerateExpenseAllocationsCommand(CurrentUser.Id, id), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}/consuntivo-detail")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConsuntivoDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConsuntivoDetail(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetConsuntivoDetailCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Budget, Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new DeleteBudgetCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }
}

public record GenerateInstallmentsFromBudgetRequest(int NumberOfInstallments, DateTime FirstDueDate, bool Force = false);
