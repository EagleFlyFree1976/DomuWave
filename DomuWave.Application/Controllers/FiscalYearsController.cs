using CPQ.Core.ActionFilters;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Command.Contabilita.Bilancio;
using DomuWave.Services.Command.FiscalYear;
using DomuWave.Services.Dto.Contabilita.Bilancio;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleMediator.Core;
using CPQ.Core.Extensions;
using DomuWave.Services.Models;

namespace DomuWave.Microservice.Controllers;

[Route("api/fiscal-years")]
public class FiscalYearsController(
    ILogger<FiscalYearsController> logger,
    IOptionsMonitor<OxCoreSettings> configuration,
    IMediator mediator)
    : TenantContextController(logger, configuration)
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("by-condominium/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(IList<FiscalYearListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetFiscalYearsByCondominiumCommand(CurrentUser.Id, condominiumId, TenantId.GetValueOrDefault()), ct);
        return Ok(result ?? []);
    }

    [HttpGet("active/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(FiscalYearReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(int condominiumId, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetActiveFiscalYearCommand(CurrentUser.Id, condominiumId), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(FiscalYearReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetFiscalYearByIdCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:int}/check-document-date")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(DocumentDateWarningDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckDocumentDate(
        int id, [FromQuery] DateTime documentDate, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new CheckFiscalYearDocumentDateCommand(CurrentUser.Id, id, documentDate), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(FiscalYearReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] FiscalYearCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new OpenFiscalYearCommand(CurrentUser.Id, dto.CondominiumId, dto.Code, dto.Description, dto.StartDate, dto.EndDate, dto.PreviousFiscalYearId), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(FiscalYearReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] FiscalYearUpdateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _mediator.GetResponse(
            new UpdateFiscalYearCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id, dto), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:int}/open")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> Open(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new OpenFiscalYearFromDraftCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/revert-to-draft")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> RevertToDraft(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new RevertFiscalYearToDraftCommand(CurrentUser.Id, id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/start-closing")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> StartClosing(int id, [FromBody] FiscalYearCloseRequestDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new StartClosingFiscalYearCommand(CurrentUser.Id, id, dto.Notes), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/close")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> Close(int id, [FromBody] FiscalYearCloseRequestDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new CloseFiscalYearCommand(CurrentUser.Id, id, dto.Notes), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/lock")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> Lock(int id, [FromBody] FiscalYearCloseRequestDto dto, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new LockFiscalYearCommand(CurrentUser.Id, id, dto.Notes), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanDelete, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new DeleteFiscalYearCommand(CurrentUser.Id, TenantId.GetValueOrDefault(), id), ct);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/propagate-opening-balances")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> PropagateOpeningBalances(int id, CancellationToken ct)
    {
        var count = await _mediator.GetResponse(
            new PropagateUnitOpeningBalancesCommand(CurrentUser.Id, id), ct);
        return Ok(new { propagatedCount = count });
    }

    [HttpGet("{id:int}/conguaglio")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ConguaglioReadDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConguaglio(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetConguaglioCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Bilancio: conto economico dell'esercizio (criterio di competenza).</summary>
    [HttpGet("{id:int}/conto-economico")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ContoEconomicoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContoEconomico(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetContoEconomicoCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Bilancio: flussi di cassa dell'esercizio (criterio di cassa).</summary>
    [HttpGet("{id:int}/flussi-cassa")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(FlussiCassaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFlussiCassa(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetFlussiCassaCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Bilancio: situazione patrimoniale alla data di fine esercizio (criterio di cassa).</summary>
    [HttpGet("{id:int}/situazione-patrimoniale")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(SituazionePatrimonialeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSituazionePatrimoniale(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetSituazionePatrimonialeCommand(CurrentUser.Id, id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Report: elenco spese dell'esercizio raggruppate per tabella millesimale.</summary>
    [HttpGet("{id:int}/report/expenses-by-millesimal")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(ExpensesByMillesimalReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpensesByMillesimalReport(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetExpensesByMillesimalReportCommand(CurrentUser.Id, id), ct);
        return Ok(result);
    }

    /// <summary>Report: bilancio di ripartizione per unità (proprietari/inquilini), con
    /// colonne dinamiche per tipo consumo e tabella millesimale, spese dirette e accrediti.</summary>
    [HttpGet("{id:int}/report/bilancio-ripartizione")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BilancioRipartizioneReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBilancioRipartizioneReport(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new GetBilancioRipartizioneReportCommand(CurrentUser.Id, id), ct);
        return Ok(result);
    }

    /// <summary>Salva in blocco gli override manuali delle celle del bilancio di ripartizione.
    /// Sostituisce il set precedente e ritorna il report ricalcolato.</summary>
    [HttpPut("{id:int}/report/bilancio-ripartizione/overrides")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BilancioRipartizioneReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveBilancioRipartizioneOverrides(
        int id, [FromBody] List<BilancioCellOverrideDto> cells, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new SaveBilancioRipartizioneOverridesCommand(CurrentUser.Id, id, cells), ct);
        return Ok(result);
    }

    /// <summary>"Ripristina automatico": cancella tutti gli override del bilancio di
    /// ripartizione dell'esercizio e ritorna il report ricalcolato.</summary>
    [HttpDelete("{id:int}/report/bilancio-ripartizione/overrides")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, AuthorizationKeys.FiscalYear, Modules.DomuWaveModule)]
    [ProducesResponseType(typeof(BilancioRipartizioneReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetBilancioRipartizioneOverrides(int id, CancellationToken ct)
    {
        var result = await _mediator.GetResponse(
            new ResetBilancioRipartizioneOverridesCommand(CurrentUser.Id, id), ct);
        return Ok(result);
    }
}
