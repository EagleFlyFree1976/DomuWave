using CPQ.Core.ActionFilters;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Command.Expense;
using DomuWave.Services.Dto.Expense;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Extensions;
namespace DomuWave.Microservice.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController(
    ILogger<ExpensesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("payment-methods")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ExpensePaymentMethodDto>))]
    public async Task<IActionResult> GetPaymentMethods(CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetExpensePaymentMethodsCommand(CurrentUser.Id), ct));

    [HttpGet("by-condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ExpenseReadDto>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetExpensesByCondominiumCommand(CurrentUser.Id, condominiumId, TenantId.GetValueOrDefault()), ct);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExpenseReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetExpenseByIdCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-condominium/{condominiumId:int}/date-range")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ExpenseReadDto>))]
    public async Task<IActionResult> GetByDateRange(
        int condominiumId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetExpensesByDateRangeCommand(CurrentUser.Id, condominiumId, from, to), ct);
        return Ok(result);
    }

    [HttpGet("by-supplier/{supplierId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ExpenseReadDto>))]
    public async Task<IActionResult> GetBySupplier(int supplierId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetExpensesBySupplierCommand(CurrentUser.Id, supplierId), ct);
        return Ok(result);
    }

    [HttpGet("by-condominium/{condominiumId:int}/type/{expenseTypeId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ExpenseReadDto>))]
    public async Task<IActionResult> GetByType(int condominiumId, int expenseTypeId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetExpensesByTypeCommand(CurrentUser.Id, condominiumId, expenseTypeId), ct);
        return Ok(result);
    }

    [HttpGet("by-condominium/{condominiumId:int}/unpaid")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ExpenseReadDto>))]
    public async Task<IActionResult> GetUnpaid(int condominiumId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetUnpaidExpensesCommand(CurrentUser.Id, condominiumId), ct);
        return Ok(result);
    }

    [HttpGet("by-condominium/{condominiumId:int}/total")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    public async Task<IActionResult> GetTotal(
        int condominiumId, [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
    {
        var total = await _mediator.GetResponse(
            new GetTotalExpensesCommand(CurrentUser.Id, condominiumId, from, to), ct);
        return Ok(new { condominiumId, from, to, total });
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ExpenseReadDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new CreateExpenseCommand(CurrentUser.Id, dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExpenseReadDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateExpenseDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateExpenseCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{id:long}/pay")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsPaid(
        long id, [FromBody] PayExpenseRequest request, CancellationToken ct)
    {
        var success = await _mediator.GetResponse(
            new MarkExpenseAsPaidCommand(CurrentUser.Id, id, request.PaymentDate, request.PaymentMethodId), ct);
        if (!success) return NotFound();
        return Ok(new { id, paid = true });
    }

    [HttpPatch("{id:long}/unpay")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsUnpaid(long id, CancellationToken ct)
    {
        var success = await _mediator.GetResponse(new MarkExpenseAsUnpaidCommand(CurrentUser.Id, id), ct);
        if (!success) return NotFound();
        return Ok(new { id, paid = false });
    }

    [HttpDelete("{id:long}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.Expenses, Modules.DomuWaveModule)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(
            new DeleteExpenseCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public record PayExpenseRequest(decimal Amount, DateTime PaymentDate, int? PaymentMethodId);
