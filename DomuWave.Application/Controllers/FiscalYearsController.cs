using CPQ.Core;
using CPQ.Core.ActionFilters;
using CPQ.Core.Controllers;
using CPQ.Core.Settings;
using DomuWave.Application.Code;
using DomuWave.Services.Models;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Interfaces;
using DomuWave.Services.Interfaces.Extensions;
using DomuWave.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DomuWave.Microservice.Controllers;

/// <summary>
/// Gestione degli esercizi condominiali (FiscalYear).
/// Un esercizio definisce il periodo temporale entro cui vengono registrati
/// spese, rate e incassi di un condominio.
/// Ciclo di vita: Open → Closing → Closed → Locked.
/// </summary>
[Route("api/fiscal-years")]
public class FiscalYearsController : PrivateAdminControllerBase 
{
    private readonly IFiscalYearService _fiscalYearService;
    private readonly ILogger<FiscalYearsController> _logger;

    public FiscalYearsController(ILogger logger, IOptionsMonitor<OxCoreSettings> configuration) : base(logger, configuration)
    {
    }

    // ─────────────────────────────────────────────
    // QUERY
    // ─────────────────────────────────────────────

    /// <summary>
    /// Restituisce tutti gli esercizi di un condominio, ordinati per data inizio discendente.
    /// </summary>
    /// <param name="condominiumId">ID del condominio.</param>
    [HttpGet("by-condominium/{condominiumId:int}")]
    
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(typeof(IList<FiscalYearListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCondominium(int condominiumId, CancellationToken ct)
    {
        var list = await _fiscalYearService.GetByCondominiumAsync(condominiumId, CurrentUser, ct);
        if (!list.Any()) return NotFound();
        var result = list.Select(fy => fy.ToListItemDto()).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Restituisce l'esercizio attualmente attivo (IsActive = true) per un condominio.
    /// </summary>
    /// <param name="condominiumId">ID del condominio.</param>
    [HttpGet("active/{condominiumId:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(typeof(FiscalYearReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActive(int condominiumId, CancellationToken ct)
    {
        var fy = await _fiscalYearService.GetActiveAsync(condominiumId, CurrentUser, ct);
        if (fy == null) return NotFound();
        return Ok(fy.ToReadDto());
    }

    /// <summary>
    /// Restituisce il dettaglio completo di un esercizio, incluso il riepilogo finanziario.
    /// </summary>
    /// <param name="id">ID dell'esercizio.</param>
    [HttpGet("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(typeof(FiscalYearReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var fy = await _fiscalYearService.GetByIdAsync(id, CurrentUser,  ct);
        if (fy == null) return NotFound();
        return Ok(fy.ToReadDto());
    }

    /// <summary>
    /// Verifica se una data documento cade fuori dal range dell'esercizio indicato.
    /// Restituisce un warning con l'esercizio suggerito per competenza.
    /// Usato dal frontend prima di confermare la registrazione di una spesa.
    /// </summary>
    /// <param name="id">ID dell'esercizio selezionato dall'utente.</param>
    /// <param name="documentDate">Data del documento (fattura, bolletta).</param>
    [HttpGet("{id:int}/check-document-date")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanView, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(typeof(DocumentDateWarningDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckDocumentDate(
        int id,
        [FromQuery] DateTime documentDate,
        CancellationToken ct)
    {
        var warning = await _fiscalYearService
            .CheckDocumentDateAsync(id, documentDate, CurrentUser, ct);
        if (warning == null) return NotFound();
        return Ok(warning);
    }

    // ─────────────────────────────────────────────
    // COMMAND — CRUD
    // ─────────────────────────────────────────────

    /// <summary>
    /// Apre un nuovo esercizio per il condominio specificato.
    /// Verifica che non esista già un esercizio Open/Closing e che le date
    /// non si sovrappongano con esercizi esistenti.
    /// </summary>
    [HttpPost]
    [AuthorizationApiFactory(AuthorizationFilterType.CanCreate, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(typeof(FiscalYearReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] FiscalYearCreateDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var fy = await _fiscalYearService.OpenAsync(
            dto.CondominiumId, dto.Code, dto.Description,
            dto.StartDate, dto.EndDate,
            CurrentUser, ct);

        return CreatedAtAction(nameof(GetById),
            new { id = fy.Id }, fy.ToReadDto());
    }

    /// <summary>
    /// Aggiorna descrizione e/o data di fine di un esercizio Open.
    /// Non è consentito modificare esercizi in stato Closed o Locked.
    /// </summary>
    [HttpPut("{id:int}")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(typeof(FiscalYearReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] FiscalYearUpdateDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _fiscalYearService
            .UpdateAsync(id, dto, CurrentUser, ct);
        if (updated == null) return NotFound();
        return Ok(updated.ToReadDto());
    }

    // ─────────────────────────────────────────────
    // COMMAND — TRANSIZIONI DI STATO
    // ─────────────────────────────────────────────

    /// <summary>
    /// Porta l'esercizio in stato Closing: segnala l'inizio della fase di chiusura.
    /// L'esercizio rimane aperto a registrazioni in rettifica (es. fatture in ritardo).
    /// </summary>
    [HttpPost("{id:int}/start-closing")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartClosing(
        int id,
        [FromBody] FiscalYearCloseRequestDto dto,
        CancellationToken ct)
    {
        var result = await _fiscalYearService
            .StartClosingAsync(id, CurrentUser, dto.Notes, ct);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Chiude definitivamente l'esercizio (Closed). IsActive passa a false.
    /// Da questo momento è possibile aprire un nuovo esercizio.
    /// Prerequisito: nessuna spesa in stato provvisorio pendente.
    /// </summary>
    [HttpPost("{id:int}/close")]
    [AuthorizationApiFactory(CPQ.Core.ActionFilters.AuthorizationFilterType.CanModify, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(
        int id,
        [FromBody] FiscalYearCloseRequestDto dto,
        CancellationToken ct)
    {
        var result = await _fiscalYearService
            .CloseAsync(id, CurrentUser, dto.Notes, ct);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Blocca definitivamente l'esercizio (Locked).
    /// Nessuna modifica è consentita dopo questo stato.
    /// Tipicamente invocato dopo l'approvazione del consuntivo in assemblea.
    /// </summary>
    [HttpPost("{id:int}/lock")]
    [AuthorizationApiFactory(AuthorizationFilterType.CanModify, "FiscalYear", Modules.Bizlio)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lock(
        int id,
        [FromBody] FiscalYearCloseRequestDto dto,
        CancellationToken ct)
    {
        var result = await _fiscalYearService
            .LockAsync(id, CurrentUser, dto.Notes, ct);
        if (!result) return NotFound();
        return NoContent();
    }
}