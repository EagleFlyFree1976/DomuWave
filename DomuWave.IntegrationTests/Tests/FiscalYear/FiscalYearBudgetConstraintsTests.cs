using System.Net;
using System.Net.Http.Json;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Models;
using FluentAssertions;
using Xunit;
using ChartOfAccountsReadDto = DomuWave.Services.Dto.Budget.ChartOfAccountsReadDto;

namespace DomuWave.IntegrationTests.Tests.FiscalYear;

/// <summary>
/// Test di integrazione per i vincoli di business tra esercizio fiscale e budget.
///
/// REGOLE DI BUSINESS VERIFICATE:
/// <list type="number">
///   <item>
///     <b>Unicità budget per tipo</b>: per ogni esercizio fiscale può esistere al massimo
///     un budget di tipo <c>Preventivo</c> e al massimo uno di tipo <c>Consuntivo</c>.
///     Un secondo tentativo con lo stesso tipo nello stesso anno restituisce 400.
///   </item>
///   <item>
///     <b>Prerequisito chiusura</b>: la transizione di stato <c>Closing → Closed</c>
///     è consentita solo se esiste un budget <c>Consuntivo</c> in stato <c>Approved</c>
///     (o <c>Closed</c>) per l'esercizio. Senza tale budget la chiusura restituisce 400.
///   </item>
/// </list>
///
/// PREREQUISITI CREATI IN <see cref="InitializeAsync"/>:
/// <list type="number">
///   <item>Condominio</item>
///   <item>Anno fiscale (stato iniziale: Draft)</item>
///   <item>Piano dei conti: un conto per tipo (Entrata, Uscita, Patrimoniale) —
///         richiesti da <c>ApproveBudgetCommandConsumer</c>.</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class FiscalYearBudgetConstraintsTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _fiscalYearId;

    private readonly List<int> _budgetIds  = [];
    private readonly List<int> _accountIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Setup: crea il contesto minimo (condominio → anno fiscale → piano dei conti).
    /// L'anno fiscale viene lasciato in stato Draft: ogni test lo porterà allo stato
    /// necessario tramite le transizioni di stato.
    /// </summary>
    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums", TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;

        var (_, fy) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years", TestDataBuilder.FiscalYear(_condominiumId));
        _fiscalYearId = fy!.Id;

        await SeedChartOfAccountsAsync();
    }

    /// <summary>
    /// Teardown best-effort in ordine inverso alle FK:
    /// budget → conti → anno fiscale → condominio.
    /// </summary>
    public override async Task DisposeAsync()
    {
        foreach (var id in _budgetIds)
        {
            try { await Client.PostAsJsonAsync($"/api/budgets/{id}/reopen", new { }); } catch { }
            try { await DeleteAsync($"/api/budgets/{id}"); }                          catch { }
        }

        foreach (var id in _accountIds)
            try { await DeleteAsync($"/api/chart-of-accounts/{id}"); } catch { }

        try { await DeleteAsync($"/api/fiscal-years/{_fiscalYearId}"); } catch { }
        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    // ── Regola 1: unicità del tipo budget per esercizio ────────────────────────

    /// <summary>
    /// Verifica che sia possibile creare esattamente un budget <c>Preventivo</c>
    /// per esercizio: il primo tentativo restituisce 201 Created, il secondo 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task CreateBudget_SecondPreventivo_SameFiscalYear_Returns400()
    {
        // Arrange: primo Preventivo (atteso ok)
        var (resp1, first) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo));
        resp1.StatusCode.Should().Be(HttpStatusCode.Created,
            "il primo budget Preventivo deve essere accettato");
        _budgetIds.Add(first!.Id);

        // Act: secondo Preventivo sullo stesso esercizio → deve essere rifiutato
        var (resp2, _) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo));

        // Assert
        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "non può esistere più di un budget Preventivo per lo stesso esercizio fiscale");
    }

    /// <summary>
    /// Verifica che sia possibile creare esattamente un budget <c>Consuntivo</c>
    /// per esercizio: il primo tentativo restituisce 201 Created, il secondo 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task CreateBudget_SecondConsuntivo_SameFiscalYear_Returns400()
    {
        // Arrange: primo Consuntivo (atteso ok)
        var (resp1, first) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Consuntivo));
        resp1.StatusCode.Should().Be(HttpStatusCode.Created,
            "il primo budget Consuntivo deve essere accettato");
        _budgetIds.Add(first!.Id);

        // Act: secondo Consuntivo sullo stesso esercizio → deve essere rifiutato
        var (resp2, _) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Consuntivo));

        // Assert
        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "non può esistere più di un budget Consuntivo per lo stesso esercizio fiscale");
    }

    /// <summary>
    /// Verifica che i due tipi di budget (Preventivo e Consuntivo) siano indipendenti:
    /// creare un Preventivo non impedisce di creare un Consuntivo e viceversa.
    /// </summary>
    [Fact]
    public async Task CreateBudget_PreventivoPlusCOnsuntivo_SameFiscalYear_BothReturn201()
    {
        // Act: crea entrambi i tipi
        var (resp1, preventivo) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo));
        var (resp2, consuntivo) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Consuntivo));

        // Assert
        resp1.StatusCode.Should().Be(HttpStatusCode.Created,
            "il Preventivo deve poter coesistere con il Consuntivo");
        resp2.StatusCode.Should().Be(HttpStatusCode.Created,
            "il Consuntivo deve poter coesistere con il Preventivo");

        _budgetIds.Add(preventivo!.Id);
        _budgetIds.Add(consuntivo!.Id);

        preventivo.Type.Should().Be(BudgetType.Preventivo);
        consuntivo.Type.Should().Be(BudgetType.Consuntivo);
    }

    // ── Regola 2: chiusura esercizio richiede Consuntivo approvato ─────────────

    /// <summary>
    /// Verifica che la chiusura dell'esercizio sia rifiutata quando non esiste
    /// alcun budget Consuntivo (né in Draft né approvato).
    /// Percorso testato: Draft → Open → Closing → <b>Close (rifiutato)</b>.
    /// </summary>
    [Fact]
    public async Task CloseFiscalYear_WithoutConsuntivoBudget_Returns400()
    {
        // Arrange: porta l'esercizio fino a Closing (senza creare alcun budget)
        await TransitionToClosingAsync();

        // Act: tenta la chiusura definitiva
        var closeResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/close", new { notes = (string?)null });

        // Assert
        closeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "la chiusura richiede un Consuntivo approvato: senza budget la richiesta deve essere rifiutata");

        var error = await ReadErrorAsync(closeResponse);
        error.Should().Contain("Consuntivo",
            "il messaggio deve indicare che manca il budget Consuntivo");
    }

    /// <summary>
    /// Verifica che la chiusura dell'esercizio sia rifiutata quando esiste un budget
    /// Consuntivo ma è ancora in stato <c>Draft</c> (non approvato).
    /// Percorso testato: Draft → Open → Closing → <b>Close (rifiutato)</b>.
    /// </summary>
    [Fact]
    public async Task CloseFiscalYear_WithDraftConsuntivoBudget_Returns400()
    {
        // Arrange: crea un Consuntivo in Draft (NON approvato) e porta l'esercizio a Closing
        var (_, consuntivo) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Consuntivo));
        _budgetIds.Add(consuntivo!.Id);

        consuntivo.StatusId.Should().Be(BudgetStatus.Draft,
            "il budget deve essere in Draft come prerequisito del test");

        await TransitionToClosingAsync();

        // Act
        var closeResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/close", new { notes = (string?)null });

        // Assert
        closeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "un Consuntivo in Draft non soddisfa il prerequisito: serve l'approvazione");

        var error = await ReadErrorAsync(closeResponse);
        error.Should().Contain("Consuntivo",
            "il messaggio deve indicare che il Consuntivo non è ancora approvato");
    }

    /// <summary>
    /// Verifica che la chiusura dell'esercizio riesca quando esiste un budget
    /// Consuntivo in stato <c>Approved</c>.
    /// Percorso testato: Draft → Open → Closing → <b>Close (200)</b>.
    /// </summary>
    [Fact]
    public async Task CloseFiscalYear_WithApprovedConsuntivoBudget_Returns204()
    {
        // Arrange: crea e approva il Consuntivo, poi porta l'esercizio a Closing
        var (_, consuntivo) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Consuntivo));
        _budgetIds.Add(consuntivo!.Id);

        var approveResponse = await ApproveBudgetAsync(consuntivo.Id);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "l'approvazione del Consuntivo deve riuscire come prerequisito del test");

        await TransitionToClosingAsync();

        // Act
        var closeResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/close", new { notes = (string?)null });

        // Assert
        closeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "con un Consuntivo approvato la chiusura deve riuscire");

        var fy = await GetAsync<FiscalYearReadDto>($"/api/fiscal-years/{_fiscalYearId}");
        fy.StatusId.Should().Be(FiscalYearStatus.Closed,
            "l'esercizio deve trovarsi in stato Closed dopo la chiusura");
    }

    /// <summary>
    /// Verifica che la presenza di un budget <c>Preventivo</c> approvato non sia
    /// sufficiente per chiudere l'esercizio: serve il <c>Consuntivo</c> approvato.
    /// Percorso testato: Draft → Open → Closing → <b>Close (rifiutato)</b>.
    /// </summary>
    [Fact]
    public async Task CloseFiscalYear_WithOnlyApprovedPreventivo_Returns400()
    {
        // Arrange: crea e approva solo il Preventivo (nessun Consuntivo)
        var (_, preventivo) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo));
        _budgetIds.Add(preventivo!.Id);

        var approveResponse = await ApproveBudgetAsync(preventivo.Id);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "l'approvazione del Preventivo deve riuscire come prerequisito del test");

        await TransitionToClosingAsync();

        // Act
        var closeResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/close", new { notes = (string?)null });

        // Assert
        closeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "il Preventivo approvato non sostituisce il requisito del Consuntivo approvato");

        var error = await ReadErrorAsync(closeResponse);
        error.Should().Contain("Consuntivo",
            "il messaggio deve indicare che manca il Consuntivo approvato");
    }

    // ── Helpers privati ────────────────────────────────────────────────────────

    /// <summary>
    /// Porta l'esercizio fiscale dallo stato corrente (Draft) fino a <c>Closing</c>
    /// tramite le transizioni Draft → Open → Closing.
    /// Ogni transizione viene verificata come prerequisito per evitare falsi negativi.
    /// </summary>
    private async Task TransitionToClosingAsync()
    {
        var openResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/open", new { });
        openResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la transizione Draft → Open deve riuscire come prerequisito del test");

        var startClosingResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/start-closing", new { notes = (string?)null });
        startClosingResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la transizione Open → Closing deve riuscire come prerequisito del test");
    }

    /// <summary>
    /// Helper per approvare un budget tramite POST /api/budgets/{id}/approve.
    /// Include i parametri richiesti dal workflow di approvazione:
    /// <c>NumberOfInstallments = 4</c> (rate trimestrali) e <c>FirstDueDate = oggi</c>.
    /// </summary>
    private Task<HttpResponseMessage> ApproveBudgetAsync(int budgetId)
        => Client.PostAsJsonAsync($"/api/budgets/{budgetId}/approve", new
        {
            NumberOfInstallments = 4,
            FirstDueDate         = DateTime.Today,
        });

    /// <summary>
    /// Seed del piano dei conti con un conto per ogni tipo richiesto
    /// da <c>ApproveBudgetCommandConsumer</c> (Entrata, Uscita, Patrimoniale).
    /// Senza almeno un conto per tipo, l'approvazione del budget fallisce.
    /// </summary>
    private async Task SeedChartOfAccountsAsync()
    {
        var types = new[]
        {
            (Type: ChartOfAccountsType.Entrata,      Name: "Entrate Test"),
            (Type: ChartOfAccountsType.Uscita,       Name: "Uscite Test"),
            (Type: ChartOfAccountsType.Patrimoniale, Name: "Patrimonio Test"),
        };

        foreach (var (type, name) in types)
        {
            var (_, acc) = await PostAsync<ChartOfAccountsReadDto>(
                "/api/chart-of-accounts",
                new CreateChartOfAccountsDto
                {
                    CondominiumId       = _condominiumId,
                    Code                = $"ACC-{TestDataBuilder.ShortId()}",
                    Name                = name,
                    Type                = type,
                    IsActive            = true,
                    AllocationMethod    = AllocationMethod.Standard,
                    ChargeabilityTypeId = ChargeabilityType.Owner,
                });

            if (acc != null)
                _accountIds.Add(acc.Id);
        }
    }
}
