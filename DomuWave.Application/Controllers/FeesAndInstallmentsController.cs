using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.CondominiumFee;
using DomuWave.Services.Command.CondominiumInstallment;
using DomuWave.Services.Dto.CondominiumFee;
using DomuWave.Services.Dto.CondominiumInstallment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
namespace DomuWave.Microservice.Controllers;

// ─── CondominiumFees ─────────────────────────────────────────────────────────

[Route("api/condominium-fees")]
[Produces("application/json")]
public class CondominiumFeesController(
    ILogger<CondominiumFeesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-installment/{installmentId:int}")]
    public async Task<IActionResult> GetByInstallment(int installmentId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetFeesByInstallmentCommand(CurrentUser.Id, installmentId), ct));

    [HttpGet("by-unit/{unitId:int}")]
    public async Task<IActionResult> GetByUnit(int unitId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetFeesByUnitCommand(CurrentUser.Id, unitId), ct));

    [HttpGet("by-user/{userId:long}")]
    public async Task<IActionResult> GetByUser(long userId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetFeesByUserCommand(CurrentUser.Id, userId), ct));

    [HttpGet("by-condominium/{condominiumId:int}/unpaid")]
    public async Task<IActionResult> GetUnpaid(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetUnpaidFeesCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("by-condominium/{condominiumId:int}/overdue")]
    public async Task<IActionResult> GetOverdue(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetOverdueFeesCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("total-due/{userId:long}")]
    public async Task<IActionResult> GetTotalDue(long userId, CancellationToken ct)
    {
        var total = await _mediator.GetResponse(new GetFeesTotalDueCommand(CurrentUser.Id, userId), ct);
        return Ok(new { userId, totalDue = total });
    }

    [HttpGet("balance/{userId:long}")]
    public async Task<IActionResult> GetBalance(long userId, CancellationToken ct)
    {
        var balance = await _mediator.GetResponse(new GetFeesTotalBalanceCommand(CurrentUser.Id, userId), ct);
        return Ok(new { userId, balance });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCondominiumFeeDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateCondominiumFeeCommand(CurrentUser.Id, dto), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCondominiumFeeDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new UpdateCondominiumFeeCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{feeId:long}/pay")]
    public async Task<IActionResult> RecordPayment(
        long feeId, [FromBody] RecordFeePaymentRequest request, CancellationToken ct)
    {
        var success = await _mediator.GetResponse(
            new RecordFeePaymentCommand(CurrentUser.Id, feeId, request.Amount, request.PaymentDate, request.PaymentMethod), ct);
        if (!success) return NotFound();
        return Ok(new { feeId, paid = true });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteCondominiumFeeCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("unit-notice/{unitId:int}/fiscal-year/{fiscalYearId:int}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> GetUnitPaymentNoticePdf(int unitId, int fiscalYearId, CancellationToken ct)
    {
        var bytes = await _mediator.GetResponse(
            new GetUnitPaymentNoticeCommand(CurrentUser.Id, unitId, fiscalYearId), ct);
        return File(bytes, "application/pdf", $"avviso-pagamento-unita-{unitId}.pdf");
    }

    [HttpGet("installment-notice/{installmentId:int}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> GetInstallmentBatchNoticePdf(int installmentId, CancellationToken ct)
    {
        var bytes = await _mediator.GetResponse(
            new GetInstallmentBatchNoticeCommand(CurrentUser.Id, installmentId), ct);
        return File(bytes, "application/pdf", $"avviso-pagamento-rata-{installmentId}.pdf");
    }

    [HttpGet("by-payment-code/{code}")]
    [ProducesResponseType(typeof(IList<FeeByPaymentCodeResultDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByPaymentCode(string code, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(new GetFeeByPaymentCodeCommand(CurrentUser.Id, code), ct);
        if (result == null || result.Count == 0) return NotFound();
        return Ok(result);
    }

    [HttpGet("unit-notice/{unitId:int}/installment/{installmentId:int}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> GetUnitInstallmentNoticePdf(int unitId, int installmentId, CancellationToken ct)
    {
        var bytes = await _mediator.GetResponse(
            new GetUnitInstallmentPaymentNoticeCommand(CurrentUser.Id, unitId, installmentId), ct);
        return File(bytes, "application/pdf", $"avviso-unita-{unitId}-rata-{installmentId}.pdf");
    }
}

public record RecordFeePaymentRequest(decimal Amount, DateTime PaymentDate, string PaymentMethod);

// ─── CondominiumInstallments ──────────────────────────────────────────────────

[Route("api/condominium-installments")]
[Produces("application/json")]
public class CondominiumInstallmentsController(
    ILogger<CondominiumInstallmentsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetInstallmentsByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("by-fiscal-year/{fiscalYearId:int}")]
    public async Task<IActionResult> GetByFiscalYear(int fiscalYearId, [FromQuery] int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetInstallmentsByFiscalYearCommand(CurrentUser.Id, condominiumId, fiscalYearId), ct));

    [HttpGet("by-condominium/{condominiumId:int}/year/{year:int}/number/{number:int}")]
    public async Task<IActionResult> GetByYearAndNumber(int condominiumId, int year, int number, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetInstallmentByYearAndNumberCommand(CurrentUser.Id, condominiumId, year, number), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-budget/{budgetId:int}")]
    public async Task<IActionResult> GetByBudget(int budgetId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetInstallmentsByBudgetCommand(CurrentUser.Id, budgetId), ct));

    [HttpGet("by-condominium/{condominiumId:int}/open")]
    public async Task<IActionResult> GetOpen(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetOpenInstallmentsCommand(CurrentUser.Id, condominiumId), ct));

    [HttpGet("by-condominium/{condominiumId:int}/overdue")]
    public async Task<IActionResult> GetOverdue(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(new GetOverdueInstallmentsCommand(CurrentUser.Id, condominiumId), ct));

    [HttpPost("by-condominium/{condominiumId:int}/generate")]
    public async Task<IActionResult> GenerateInstallments(
        int condominiumId, [FromBody] GenerateInstallmentsRequest request, CancellationToken ct)
    {
        var success = await _mediator.GetResponse(
            new GenerateInstallmentsCommand(CurrentUser.Id, condominiumId, request.FiscalYearId, request.BudgetId), ct);
        if (!success) return BadRequest(new { error = "Impossibile generare le rate per i parametri forniti." });
        return Ok(new { condominiumId, fiscalYearId = request.FiscalYearId, generated = true });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCondominiumInstallmentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(new CreateCondominiumInstallmentCommand(CurrentUser.Id, dto), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCondominiumInstallmentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateCondominiumInstallmentCommand(CurrentUser.Id, id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.GetResponse(new DeleteCondominiumInstallmentCommand(CurrentUser.Id, id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public record GenerateInstallmentsRequest(int FiscalYearId, int BudgetId);
