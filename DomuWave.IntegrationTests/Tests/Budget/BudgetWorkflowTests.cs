using System.Net.Http.Json;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Models;
using FluentAssertions;
using System.Net;
using Xunit;
using ChartOfAccountsReadDto = DomuWave.Services.Dto.Budget.ChartOfAccountsReadDto;

namespace DomuWave.IntegrationTests.Tests.Budget;

/// <summary>
/// Test di integrazione per il pipeline CQRS del Budget e per il workflow di stato
/// Draft → Approve → Close (con Reopen come operazione inversa).
///
/// PREREQUISITI: ogni esecuzione della classe crea in <see cref="InitializeAsync"/>:
/// <list type="number">
///   <item>Un condominio (<c>_condominiumId</c>)</item>
///   <item>Un anno fiscale (<c>_fiscalYearId</c>)</item>
///   <item>Un set di conti del piano dei conti (<c>_accountIds</c>): Entrata, Uscita, Patrimoniale.
///         Sono necessari perché <c>ApproveBudgetCommandConsumer</c> verifica che esista
///         almeno un conto per tipo prima di procedere all'approvazione.</item>
/// </list>
/// Tutto viene eliminato in <see cref="DisposeAsync"/> in ordine inverso (budget → conti → anno → condominio).
///
/// ENDPOINT TESTATI (da BudgetsController):
/// <code>
///   POST   /api/budgets               → 201 Created
///   GET    /api/budgets/{id}          → 200 OK | 404 Not Found
///   GET    /api/budgets/by-condominium/{id} → 200 OK
///   PUT    /api/budgets/{id}          → 200 OK
///   POST   /api/budgets/{id}/approve  → 204 No Content | 400 Bad Request
///   POST   /api/budgets/{id}/close    → 204 No Content | 400 Bad Request
///   POST   /api/budgets/{id}/reopen   → 204 No Content
///   DELETE /api/budgets/{id}          → 204 No Content | 400 Bad Request
/// </code>
///
/// STATI DEL BUDGET: Draft (1) → Approved (2) → Closed (3). Reopen riporta da Approved a Draft.
/// TIPI DI BUDGET: Preventivo (1), Consuntivo (2). Un solo budget per tipo per condominio+anno.
/// </summary>
[Collection("Integration")]
public class BudgetWorkflowTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _fiscalYearId;

    /// <summary>Id dei budget creati durante i test, per il cleanup in DisposeAsync.</summary>
    private readonly List<int> _budgetIds  = [];

    /// <summary>Id dei conti del piano dei conti creati nel seed, per il cleanup.</summary>
    private readonly List<int> _accountIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────

    /// <summary>
    /// Setup: crea condominio → anno fiscale → piano dei conti (seed minimo).
    /// Il piano dei conti è necessario perché l'approvazione del budget verifica
    /// che esistano conti di tipo Entrata, Uscita e Patrimoniale.
    /// </summary>
    public override async Task InitializeAsync()
    {
        // 1. Condominio di contesto
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums",
            TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;

        // 2. Anno fiscale per il condominio
        var (_, fy) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years",
            TestDataBuilder.FiscalYear(_condominiumId));
        _fiscalYearId = fy!.Id;

        // 3. Seed piano dei conti (almeno 1 conto per ogni tipo richiesto dall'approvazione)
        await SeedChartOfAccountsAsync();
    }

    /// <summary>
    /// Teardown best-effort in ordine inverso alle FK:
    /// budget → conti del piano dei conti → anno fiscale → condominio.
    /// Ogni step usa try/catch per non bloccare il cleanup successivo.
    /// </summary>
    public override async Task DisposeAsync()
    {
        foreach (var id in _budgetIds)
        {
            try { await DeleteAsync($"/api/budgets/{id}"); }
            catch { /* ignora */ }
        }

        foreach (var id in _accountIds)
        {
            try { await DeleteAsync($"/api/chart-of-accounts/{id}"); }
            catch { /* ignora */ }
        }

        try { await DeleteAsync($"/api/fiscal-years/{_fiscalYearId}"); }
        catch { /* ignora */ }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); }
        catch { /* ignora */ }

        await base.DisposeAsync();
    }

    // ── POST /api/budgets ──────────────────────────────────────────────────

    /// <summary>
    /// Verifica che la creazione di un budget Preventivo restituisca 201 Created
    /// con stato Draft (1), tipo Preventivo (1) e Id generato > 0.
    /// Lo stato Draft è l'unico consentito alla creazione: il workflow richiede
    /// l'approvazione esplicita prima che il budget sia operativo.
    /// </summary>
    [Fact]
    public async Task Create_PreventivoBudget_Returns201InDraftStatus()
    {
        // Arrange
        var dto = TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo);

        // Act
        var (response, created) = await PostAsync<BudgetReadDto>("/api/budgets", dto);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.StatusId.Should().Be(BudgetStatus.Draft, "i budget devono partire in stato Draft");
        created.Type.Should().Be(BudgetType.Preventivo);

        _budgetIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che la creazione di un budget Consuntivo restituisca 201 Created
    /// con stato Draft e tipo Consuntivo (2).
    /// Il Consuntivo è il rendiconto a fine anno (vs il Preventivo che è la previsione).
    /// </summary>
    [Fact]
    public async Task Create_ConsuntivoBudget_Returns201InDraftStatus()
    {
        // Arrange
        var dto = TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Consuntivo);

        // Act
        var (response, created) = await PostAsync<BudgetReadDto>("/api/budgets", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created!.StatusId.Should().Be(BudgetStatus.Draft);
        created.Type.Should().Be(BudgetType.Consuntivo);

        _budgetIds.Add(created.Id);
    }

    // ── GET /api/budgets/{id} ──────────────────────────────────────────────

    /// <summary>
    /// Verifica che la GET per id di un budget esistente restituisca 200 OK con
    /// Id, CondominiumId e FiscalYearId corretti.
    /// </summary>
    [Fact]
    public async Task GetById_ExistingBudget_ReturnsBudget()
    {
        // Arrange: crea un budget e recupera il suo Id
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        // Act
        var result = await GetAsync<BudgetReadDto>($"/api/budgets/{created.Id}");

        // Assert
        result.Id.Should().Be(created.Id);
        result.CondominiumId.Should().Be(_condominiumId);
        result.FiscalYearId.Should().Be(_fiscalYearId);
    }

    /// <summary>
    /// Verifica che la GET per un id inesistente (999999) restituisca 404 Not Found.
    /// </summary>
    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await Client.GetAsync("/api/budgets/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifica che la GET per condominio restituisca una lista che include il budget
    /// appena creato. Altri budget precedenti potrebbero essere presenti nella lista.
    /// </summary>
    [Fact]
    public async Task GetByCondominium_IncludesCreatedBudget()
    {
        // Arrange
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        // Act
        var list = await GetAsync<IList<BudgetReadDto>>(
            $"/api/budgets/by-condominium/{_condominiumId}");

        // Assert: la lista deve contenere il budget appena creato
        list.Should().Contain(b => b.Id == created.Id);
    }

    // ── Workflow: Draft → Approve ──────────────────────────────────────────

    /// <summary>
    /// Verifica il primo step del workflow: Draft → Approved.
    /// L'approvazione restituisce 204 NoContent e il budget risultante ha stato Approved (2).
    /// L'approvazione richiede anche il numero di rate e la prima scadenza (per la generazione
    /// automatica delle rate tramite <c>GenerateInstallmentsAsync</c>).
    /// </summary>
    [Fact]
    public async Task Approve_BudgetInDraft_Returns204AndStatusBecomesApproved()
    {
        // Arrange: crea un budget in Draft
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        // Act: approva il budget
        var approveResponse = await ApproveBudgetAsync(created.Id);

        // Assert: 204 NoContent per l'approvazione
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "l'approvazione con successo deve restituire 204 NoContent");

        // Verifica che lo stato nel DB sia cambiato ad Approved
        var budget = await GetAsync<BudgetReadDto>($"/api/budgets/{created.Id}");
        budget.StatusId.Should().Be(BudgetStatus.Approved);
    }

    /// <summary>
    /// Verifica che il tentativo di approvare un budget già approvato restituisca
    /// 400 Bad Request. Il workflow non consente transizioni di stato invalide
    /// (Approved → Approved non è una transizione valida).
    /// </summary>
    [Fact]
    public async Task Approve_AlreadyApprovedBudget_Returns400()
    {
        // Arrange: crea e approva il budget
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        await ApproveBudgetAsync(created.Id); // prima approvazione (ok)

        // Act: seconda approvazione — deve fallire
        var secondApprove = await ApproveBudgetAsync(created.Id);

        // Assert
        secondApprove.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifica la regola di business sull'unicità del tipo di budget per condominio + anno fiscale:
    /// non possono esistere due budget di tipo Preventivo per lo stesso condominio nello stesso
    /// anno fiscale. Il secondo tentativo deve essere rifiutato con 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task Create_DuplicateTypeSameFiscalYear_Returns400()
    {
        // Arrange: crea il primo Preventivo (ok)
        var (_, first) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo));
        _budgetIds.Add(first!.Id);

        // Act: secondo Preventivo sullo stesso condominio+anno fiscale → rifiutato
        var (secondResponse, _) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo));
        var secondBody = await secondResponse.Content.ReadAsStringAsync();

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, secondBody);
    }

    // ── Workflow: Approve → Close ──────────────────────────────────────────

    /// <summary>
    /// Verifica il secondo step del workflow: Approved → Closed.
    /// La chiusura restituisce 204 NoContent e il budget risultante ha stato Closed (3).
    /// Un budget chiuso è definitivo e non può essere modificato.
    /// </summary>
    [Fact]
    public async Task Close_ApprovedBudget_Returns204AndStatusBecomesClosed()
    {
        // Arrange: crea e approva il budget
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        await ApproveBudgetAsync(created.Id);

        // Act: chiude il budget approvato
        var closeResponse = await Client.PostAsJsonAsync(
            $"/api/budgets/{created.Id}/close", new { });

        // Assert
        closeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var budget = await GetAsync<BudgetReadDto>($"/api/budgets/{created.Id}");
        budget.StatusId.Should().Be(BudgetStatus.Closed);
    }

    /// <summary>
    /// Verifica che il tentativo di chiudere un budget in stato Draft (saltando l'approvazione)
    /// restituisca 400 Bad Request. Il workflow impone la transizione Draft → Approved → Closed.
    /// </summary>
    [Fact]
    public async Task Close_DraftBudget_Returns400()
    {
        // Arrange: crea un budget ma NON lo approva
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        // Act: tenta di chiudere direttamente da Draft (salto dell'approvazione → invalido)
        var response = await Client.PostAsJsonAsync(
            $"/api/budgets/{created.Id}/close", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Workflow: Approve → Reopen ─────────────────────────────────────────

    /// <summary>
    /// Verifica l'operazione di riapertura: Approved → Draft.
    /// La riapertura è utile quando si scopre un errore nel budget approvato
    /// e si deve correggerlo prima della chiusura definitiva.
    /// Restituisce 204 NoContent e il budget risultante torna in stato Draft.
    /// </summary>
    [Fact]
    public async Task Reopen_ApprovedBudget_Returns204AndStatusReturnsToDraft()
    {
        // Arrange: crea e approva il budget
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        await ApproveBudgetAsync(created.Id);

        // Act: riapre il budget approvato
        var reopenResponse = await Client.PostAsJsonAsync(
            $"/api/budgets/{created.Id}/reopen", new { });
        var body = await reopenResponse.Content.ReadAsStringAsync();

        // Assert
        reopenResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verifica che lo stato sia tornato a Draft
        var budget = await GetAsync<BudgetReadDto>($"/api/budgets/{created.Id}");
        budget.StatusId.Should().Be(BudgetStatus.Draft);
    }

    // ── DELETE /api/budgets/{id} ───────────────────────────────────────────

    /// <summary>
    /// Verifica che l'eliminazione di un budget in stato Draft restituisca 204 NoContent.
    /// Solo i budget in Draft possono essere eliminati (non hanno effetti contabili).
    /// Non aggiunge l'Id a <c>_budgetIds</c> perché il test stesso lo elimina.
    /// </summary>
    [Fact]
    public async Task Delete_DraftBudget_Returns204()
    {
        // Arrange
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));

        // Act — elimina il budget in Draft (non aggiunto a _budgetIds)
        var response = await DeleteAsync($"/api/budgets/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Verifica che il tentativo di eliminare un budget già approvato restituisca
    /// 400 Bad Request. I budget approvati non possono essere eliminati perché hanno
    /// generato le rate condominiali e sono tracciabili contabilmente.
    /// </summary>
    [Fact]
    public async Task Delete_ApprovedBudget_Returns400()
    {
        // Arrange: crea e approva il budget
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id); // registra per cleanup (l'eliminazione fallirà)

        await ApproveBudgetAsync(created.Id);

        // Act: tenta di eliminare il budget approvato
        var response = await DeleteAsync($"/api/budgets/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "i budget approvati non possono essere eliminati");
    }

    // ── PUT /api/budgets/{id} ──────────────────────────────────────────────

    /// <summary>
    /// Verifica che la modifica di un budget in stato Draft restituisca 200 OK con
    /// il DTO aggiornato. Solo i budget in Draft sono modificabili; quelli approvati
    /// o chiusi sono in sola lettura.
    /// </summary>
    [Fact]
    public async Task Update_DraftBudget_Returns200()
    {
        // Arrange: crea un budget in Draft
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        var updateDto = new UpdateBudgetDto { Notes = "Note aggiornate" };

        // Act
        var (response, updated) = await PutAsync<BudgetReadDto>(
            $"/api/budgets/{created.Id}", updateDto);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Notes.Should().Be(updateDto.Notes); // verifica che le note siano aggiornate
    }

    // ── Helpers privati ────────────────────────────────────────────────────

    /// <summary>
    /// Helper per approvare un budget tramite POST /api/budgets/{id}/approve.
    /// Include i parametri richiesti dal workflow di approvazione:
    /// <c>NumberOfInstallments = 4</c> (rate trimestrali) e <c>FirstDueDate = oggi</c>.
    /// Restituisce il <see cref="HttpResponseMessage"/> grezzo per consentire l'asserzione
    /// sul codice di stato (204 se ok, 400 se lo stato non lo consente).
    /// </summary>
    private Task<HttpResponseMessage> ApproveBudgetAsync(int budgetId)
        => Client.PostAsJsonAsync($"/api/budgets/{budgetId}/approve", new
        {
            NumberOfInstallments = 4,          // 4 rate trimestrali
            FirstDueDate         = DateTime.Today,
        });

    /// <summary>
    /// Seed del piano dei conti con un conto per ogni tipo richiesto
    /// da <c>ApproveBudgetCommandConsumer</c>: Entrata, Uscita, Patrimoniale.
    /// Senza almeno un conto per tipo, l'approvazione del budget fallisce.
    /// I codici dei conti sono univoci per evitare conflitti tra esecuzioni.
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
                    CondominiumId = _condominiumId,
                    Code          = $"ACC-{TestDataBuilder.ShortId()}", // codice univoco
                    Name          = name,
                    Type          = type,
                    IsActive      = true,
                });

            if (acc != null)
                _accountIds.Add(acc.Id);
        }
    }
}
