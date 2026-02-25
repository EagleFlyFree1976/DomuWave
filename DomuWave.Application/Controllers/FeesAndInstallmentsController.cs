using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Interfaces;
using CPQ.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

// ═══════════════════════════════════════════════════════════════════════════
// Rate (CondominiumFees)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Gestione Rate Condominiali per unità e condominio.
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class CondominiumFeesController(
    ILogger<CondominiumFeesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    ICondominiumFeeService feeService)
    : PrivateControllerBase(logger, configuration)
{
    /// <summary>
    /// Restituisce tutte le rate di una bolletta/rata condominiale.
    /// </summary>
    [HttpGet("by-installment/{installmentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumFee>))]
    public async Task<IActionResult> GetByInstallment(int installmentId, CancellationToken cancellationToken)
    {
        var result = await feeService.GetByInstallmentIdAsync(installmentId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce le rate di una specifica unità immobiliare.
    /// </summary>
    [HttpGet("by-unit/{unitId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumFee>))]
    public async Task<IActionResult> GetByUnit(int unitId, CancellationToken cancellationToken)
    {
        var result = await feeService.GetByUnitIdAsync(unitId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce le rate associate a un utente (proprietario/inquilino).
    /// </summary>
    [HttpGet("by-user/{userId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumFee>))]
    public async Task<IActionResult> GetByUser(long userId, CancellationToken cancellationToken)
    {
        var result = await feeService.GetByUserIdAsync(userId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce le rate non pagate del condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/unpaid")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumFee>))]
    public async Task<IActionResult> GetUnpaid(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await feeService.GetUnpaidFeesAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce le rate scadute del condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/overdue")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumFee>))]
    public async Task<IActionResult> GetOverdue(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await feeService.GetOverdueFeesAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calcola il totale dovuto da un utente.
    /// </summary>
    [HttpGet("total-due/{userId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalDue(long userId, CancellationToken cancellationToken)
    {
        var total = await feeService.GetTotalDueAsync(userId, CurrentUser, cancellationToken);
        return Ok(new { userId, totalDue = total });
    }

    /// <summary>
    /// Restituisce il saldo (pagamenti effettuati) di un utente.
    /// </summary>
    [HttpGet("balance/{userId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance(long userId, CancellationToken cancellationToken)
    {
        var balance = await feeService.GetTotalBalanceAsync(userId, CurrentUser, cancellationToken);
        return Ok(new { userId, balance });
    }

    /// <summary>
    /// Crea una nuova rata condominiale.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CondominiumFee))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CondominiumFee fee, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await feeService.CreateAsync(fee, CurrentUser, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Registra il pagamento di una rata.
    /// </summary>
    [HttpPatch("{feeId:long}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordPayment(
        long feeId,
        [FromBody] RecordFeePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var success = await feeService.RecordPaymentAsync(
            feeId, request.Amount, request.PaymentDate,
            request.PaymentMethod, CurrentUser.Id, CurrentUser, cancellationToken);

        if (!success) return NotFound();
        return Ok(new { feeId, paid = true });
    }

    /// <summary>
    /// Soft-delete di una rata.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await feeService.DeleteAsync(id, CurrentUser, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

/// <summary>Request body per la registrazione di un pagamento rata.</summary>
public record RecordFeePaymentRequest(decimal Amount, DateTime PaymentDate, string PaymentMethod);

// ═══════════════════════════════════════════════════════════════════════════
// Bollette/Periodi (CondominiumInstallments)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Gestione Periodi (bollette) del condominio.
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class CondominiumInstallmentsController(
    ILogger<CondominiumInstallmentsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    ICondominiumInstallmentService installmentService)
    : PrivateControllerBase(logger, configuration)
{
    /// <summary>
    /// Restituisce tutti i periodi di un condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumInstallment>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await installmentService.GetByCondominiumIdAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce un periodo specifico per anno e numero.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/year/{year:int}/number/{number:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumInstallment))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByYearAndNumber(
        int condominiumId, int year, int number, CancellationToken cancellationToken)
    {
        var result = await installmentService.GetByYearAndNumberAsync(condominiumId, year, number, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Restituisce i periodi aperti del condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/open")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumInstallment>))]
    public async Task<IActionResult> GetOpen(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await installmentService.GetOpenInstallmentsAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce i periodi scaduti del condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/overdue")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CondominiumInstallment>))]
    public async Task<IActionResult> GetOverdue(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await installmentService.GetOverdueInstallmentsAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Genera automaticamente i periodi di un anno a partire da un budget.
    /// </summary>
    [HttpPost("by-condominium/{condominiumId:int}/generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateInstallments(
        int condominiumId,
        [FromBody] GenerateInstallmentsRequest request,
        CancellationToken cancellationToken)
    {
        var success = await installmentService.GenerateInstallmentsAsync(
            condominiumId, request.Year, request.BudgetId,
            CurrentUser.Id, CurrentUser, cancellationToken);

        if (!success) return BadRequest(new { error = "Impossibile generare le rate per i parametri forniti." });
        return Ok(new { condominiumId, year = request.Year, generated = true });
    }

    /// <summary>
    /// Crea un nuovo periodo condominiale.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CondominiumInstallment))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CondominiumInstallment installment, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await installmentService.CreateAsync(installment, CurrentUser, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Aggiorna un periodo esistente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CondominiumInstallment))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CondominiumInstallment installment, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var exists = await installmentService.ExistsAsync(id, CurrentUser, cancellationToken);
        if (!exists) return NotFound();
        installment.Id = id;
        var result = await installmentService.UpdateAsync(installment, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Soft-delete di un periodo.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await installmentService.DeleteAsync(id, CurrentUser, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

/// <summary>Request body per la generazione automatica delle rate.</summary>
public record GenerateInstallmentsRequest(int Year, int BudgetId);
