using CPQ.Core.Extensions;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.ElectronicInvoices;
using DomuWave.Services.Dto.ElectronicInvoice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Fatture elettroniche passive: download massivo dal Cassetto Fiscale / SdI (via provider
/// accreditato), elenco e collegamento alle spese. Riservato all'amministratore.
/// </summary>
[Route("api/electronic-invoices")]
[Produces("application/json")]
public class ElectronicInvoicesController(
    ILogger<ElectronicInvoicesController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : PrivateControllerBase(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    /// <summary>Restituisce la configurazione del download fatture per un condominio (senza chiave API).</summary>
    [HttpGet("config/{condominiumId:int}")]
    [ProducesResponseType(typeof(EInvoiceConfigReadDto), 200)]
    public async Task<IActionResult> GetConfig(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(
            new GetEInvoiceConfigCommand(CurrentUser.Id, condominiumId), ct));

    /// <summary>Salva provider, P.IVA ed (eventuale) chiave API per un condominio.</summary>
    [HttpPut("config/{condominiumId:int}")]
    [ProducesResponseType(typeof(EInvoiceConfigReadDto), 200)]
    public async Task<IActionResult> UpdateConfig(int condominiumId, [FromBody] EInvoiceConfigUpdateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateEInvoiceConfigCommand(CurrentUser.Id, condominiumId, dto), ct);
        return Ok(result);
    }

    /// <summary>Scarica le fatture passive del condominio nell'intervallo indicato. Restituisce le nuove fatture importate.</summary>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(IList<ElectronicInvoiceReadDto>), 200)]
    public async Task<IActionResult> Sync([FromBody] SyncEInvoicesDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new SyncEInvoicesCommand(CurrentUser.Id, dto.CondominiumId, dto.From, dto.To), ct);
        return Ok(result);
    }

    /// <summary>Elenca le fatture elettroniche scaricate per un condominio.</summary>
    [HttpGet("by-condominium/{condominiumId:int}")]
    [ProducesResponseType(typeof(IList<ElectronicInvoiceReadDto>), 200)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
        => Ok(await _mediator.GetResponse(
            new GetEInvoicesByCondominiumCommand(CurrentUser.Id, condominiumId), ct));

    /// <summary>Collega una fattura scaricata a una spesa esistente.</summary>
    [HttpPost("{id:int}/link-expense/{expenseId:long}")]
    [ProducesResponseType(typeof(ElectronicInvoiceReadDto), 200)]
    public async Task<IActionResult> LinkExpense(int id, long expenseId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new LinkEInvoiceToExpenseCommand(CurrentUser.Id, id, expenseId), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
