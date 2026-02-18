using CPQ.Core.Memberships;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Domain.Models;
using DomuWave.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione Spese condominiali.
/// </summary>
[Route("api/[controller]")]
[Produces("application/json")]
public class ExpensesController(
    ILogger<ExpensesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IExpenseService expenseService)
    : PrivateControllerBase(logger, configuration)
{
    /// <summary>
    /// Restituisce tutte le spese di un condominio.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Expense>))]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await expenseService.GetByCondominiumIdAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce una spesa per ID.
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Expense))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await expenseService.GetByIdAsync(id, CurrentUser, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Filtra spese per intervallo di date.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/date-range")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Expense>))]
    public async Task<IActionResult> GetByDateRange(
        int condominiumId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var result = await expenseService.GetByDateRangeAsync(condominiumId, from, to, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Filtra spese per fornitore.
    /// </summary>
    [HttpGet("by-supplier/{supplierId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Expense>))]
    public async Task<IActionResult> GetBySupplier(int supplierId, CancellationToken cancellationToken)
    {
        var result = await expenseService.GetBySupplierIdAsync(supplierId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Filtra spese per tipo.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/type/{expenseType}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Expense>))]
    public async Task<IActionResult> GetByType(int condominiumId, string expenseType, CancellationToken cancellationToken)
    {
        var result = await expenseService.GetByTypeAsync(condominiumId, expenseType, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Restituisce spese non ancora pagate.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/unpaid")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Expense>))]
    public async Task<IActionResult> GetUnpaid(int condominiumId, CancellationToken cancellationToken)
    {
        var result = await expenseService.GetUnpaidExpensesAsync(condominiumId, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calcola il totale delle spese in un periodo.
    /// </summary>
    [HttpGet("by-condominium/{condominiumId:int}/total")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotal(
        int condominiumId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var total = await expenseService.GetTotalExpensesAsync(condominiumId, from, to, CurrentUser, cancellationToken);
        return Ok(new { condominiumId, from, to, total });
    }

    /// <summary>
    /// Crea una nuova spesa.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Expense))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Expense expense, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await expenseService.CreateAsync(expense, CurrentUser, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Aggiorna una spesa esistente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Expense))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] Expense expense, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var exists = await expenseService.ExistsAsync(id, CurrentUser, cancellationToken);
        if (!exists) return NotFound();
        expense.Id = id;
        var result = await expenseService.UpdateAsync(expense, CurrentUser, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra il pagamento di una spesa.
    /// </summary>
    [HttpPatch("{id:long}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsPaid(
        long id,
        [FromBody] PayExpenseRequest request,
        CancellationToken cancellationToken)
    {
        
        var success = await expenseService.MarkAsPaidAsync(id, request.PaymentDate, request.PaymentMethod,
             CurrentUser, cancellationToken);

        if (!success) return NotFound();
        return Ok(new { id, paid = true });
    }

    /// <summary>
    /// Soft-delete di una spesa.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await expenseService.DeleteAsync(id, CurrentUser, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

/// <summary>Request body per il pagamento di una spesa.</summary>
public record PayExpenseRequest(decimal Amount, DateTime PaymentDate, string PaymentMethod);
