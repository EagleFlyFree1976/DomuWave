using System.Net;
using System.Net.Http.Json;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Dto.Expense;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Models;
using FluentAssertions;
using Xunit;
using ChartOfAccountsReadDto = DomuWave.Services.Dto.Budget.ChartOfAccountsReadDto;

namespace DomuWave.IntegrationTests.Tests.Expense;

/// <summary>
/// Test di integrazione per la business rule che vieta la registrazione di spese
/// su esercizi fiscali in stato non operativo.
///
/// REGOLA DI BUSINESS:
/// Le spese (uscite) possono essere registrate solo su esercizi in stato
/// <c>Open</c> (2) o <c>Closing</c> (3).
/// Gli stati <c>Draft</c> (1), <c>Closed</c> (4) e <c>Locked</c> (5) non consentono
/// nuove registrazioni:
/// <list type="bullet">
///   <item><b>Draft</b>: l'esercizio non è ancora operativo; nessuna operazione contabile è consentita.</item>
///   <item><b>Closed</b>: l'esercizio è stato chiuso definitivamente; i movimenti storici sono immutabili.</item>
///   <item><b>Locked</b>: l'esercizio è bloccato in modo irreversibile; nessuna modifica è consentita.</item>
/// </list>
///
/// PREREQUISITI CREATI IN <see cref="InitializeAsync"/>:
/// <list type="number">
///   <item>Condominio</item>
///   <item>Anno fiscale (partirà in stato Draft)</item>
///   <item>Piano dei conti: 1 conto Uscita (usato come AccountId nella spesa) +
///         1 conto Entrata + 1 conto Patrimoniale (richiesti dall'approvazione del budget)</item>
///   <item>Tabella millesimale (richiesta dalla spesa)</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class ExpenseFiscalYearStatusTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _fiscalYearId;
    private int _accountUscitaId;
    private int _millesimalTableId;

    private readonly List<int>  _budgetIds  = [];
    private readonly List<int>  _accountIds = [];
    private readonly List<long> _expenseIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Setup: crea il contesto minimo per registrare una spesa.
    /// L'esercizio viene creato in stato Draft; ogni test lo porterà allo stato
    /// desiderato tramite le transizioni di stato.
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
        await SeedMillesimalTableAsync();
    }

    /// <summary>
    /// Teardown best-effort in ordine inverso alle FK:
    /// spese → budget → conti → tabella millesimale → anno fiscale → condominio.
    /// </summary>
    public override async Task DisposeAsync()
    {
        foreach (var id in _expenseIds)
            try { await DeleteAsync($"/api/expenses/{id}"); } catch { }

        foreach (var id in _budgetIds)
        {
            try { await Client.PostAsJsonAsync($"/api/budgets/{id}/reopen", new { }); } catch { }
            try { await DeleteAsync($"/api/budgets/{id}"); }                          catch { }
        }

        foreach (var id in _accountIds)
            try { await DeleteAsync($"/api/chart-of-accounts/{id}"); } catch { }

        try { await DeleteAsync($"/api/millesimal-tables/{_millesimalTableId}"); } catch { }
        try { await DeleteAsync($"/api/fiscal-years/{_fiscalYearId}"); }          catch { }
        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); }          catch { }

        await base.DisposeAsync();
    }

    // ── Test ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che non sia possibile registrare una spesa su un esercizio in stato
    /// <c>Draft</c>: l'esercizio non è ancora aperto alle registrazioni contabili.
    /// </summary>
    [Fact]
    public async Task CreateExpense_FiscalYearDraft_Returns400()
    {
        // Arrange: l'esercizio è già in stato Draft (creato in InitializeAsync)
        var fy = await GetAsync<FiscalYearReadDto>($"/api/fiscal-years/{_fiscalYearId}");
        fy.StatusId.Should().Be(FiscalYearStatus.Draft,
            "l'esercizio deve essere in stato Draft come prerequisito del test");

        // Act
        var (response, _) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "non è consentito registrare spese su un esercizio in stato Draft");

        var error = await ReadErrorAsync(response);
        error.Should().Contain("Bozza",
            "il messaggio deve indicare che l'esercizio è in stato Bozza");
    }

    /// <summary>
    /// Verifica che sia possibile registrare una spesa su un esercizio in stato
    /// <c>Open</c>: è lo stato operativo normale dell'esercizio.
    /// </summary>
    [Fact]
    public async Task CreateExpense_FiscalYearOpen_Returns201()
    {
        // Arrange: porta l'esercizio in stato Open (Draft → Open)
        await OpenFiscalYearAsync();

        // Act
        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "le spese possono essere registrate su un esercizio in stato Open");
        created.Should().NotBeNull();
        if (created != null) _expenseIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che sia possibile registrare una spesa su un esercizio in stato
    /// <c>Closing</c>: la fase di chiusura permette ancora rettifiche e registrazioni tardive.
    /// </summary>
    [Fact]
    public async Task CreateExpense_FiscalYearClosing_Returns201()
    {
        // Arrange: porta l'esercizio in stato Closing (Draft → Open → Closing)
        await OpenFiscalYearAsync();
        var startClosingResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/start-closing", new { notes = (string?)null });
        startClosingResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la transizione Open → Closing deve riuscire come prerequisito del test");

        // Act
        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "le spese possono essere registrate su un esercizio in stato Closing");
        created.Should().NotBeNull();
        if (created != null) _expenseIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che non sia possibile registrare una spesa su un esercizio in stato
    /// <c>Closed</c>: la chiusura definitiva congela i movimenti contabili.
    /// </summary>
    [Fact]
    public async Task CreateExpense_FiscalYearClosed_Returns400()
    {
        // Arrange: porta l'esercizio in stato Closed (Draft → Open → Closing → Closed)
        // La chiusura richiede un Consuntivo approvato come prerequisito.
        await OpenFiscalYearAsync();
        await ApproveConsuntivoAndCloseAsync();

        // Act
        var (response, _) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "non è consentito registrare spese su un esercizio chiuso");

        var error = await ReadErrorAsync(response);
        error.Should().Contain("chiuso",
            "il messaggio deve indicare che l'esercizio è chiuso");
    }

    /// <summary>
    /// Verifica che non sia possibile registrare una spesa su un esercizio in stato
    /// <c>Locked</c>: il blocco è irreversibile e congela definitivamente ogni modifica.
    /// </summary>
    [Fact]
    public async Task CreateExpense_FiscalYearLocked_Returns400()
    {
        // Arrange: porta l'esercizio in stato Locked
        // (Draft → Open → Closing → Closed → Locked)
        // La chiusura richiede un Consuntivo approvato come prerequisito.
        await OpenFiscalYearAsync();
        await ApproveConsuntivoAndCloseAsync();

        var lockResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/lock", new { notes = (string?)null });
        lockResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "il blocco dell'esercizio deve riuscire come prerequisito del test");

        // Act
        var (response, _) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "non è consentito registrare spese su un esercizio bloccato");

        var error = await ReadErrorAsync(response);
        error.Should().Contain("bloccato",
            "il messaggio deve indicare che l'esercizio è bloccato");
    }

    /// <summary>
    /// Verifica che ogni spesa creata sia associata a un esercizio fiscale:
    /// il campo <c>FiscalYearId</c> nel DTO di risposta deve essere non nullo e
    /// coincidere con l'esercizio fiscale dell'anno corrente.
    /// </summary>
    [Fact]
    public async Task CreateExpense_WithExplicitFiscalYear_HasFiscalYearAssociated()
    {
        // Arrange
        await OpenFiscalYearAsync();

        // Act
        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.FiscalYearId.Should().Be(_fiscalYearId,
            "ogni spesa deve essere associata all'esercizio fiscale specificato");
        created.FiscalYearCode.Should().NotBeNullOrEmpty(
            "il codice dell'esercizio fiscale deve essere valorizzato nel DTO di risposta");

        _expenseIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che ogni spesa creata senza <c>FiscalYearId</c> esplicito venga comunque
    /// associata all'esercizio fiscale attivo del condominio (risoluzione automatica).
    /// </summary>
    [Fact]
    public async Task CreateExpense_WithoutExplicitFiscalYear_AutoResolvesActiveFiscalYear()
    {
        // Arrange
        await OpenFiscalYearAsync();

        var expenseWithoutFiscalYear = new CreateExpenseDto
        {
            CondominiumId       = _condominiumId,
            FiscalYearId        = null,   // omesso — deve essere risolto automaticamente
            AccountId           = _accountUscitaId,
            MillesimalTableId   = _millesimalTableId,
            Name                = $"Spesa auto-fy {TestDataBuilder.ShortId()}",
            DocumentDate        = DateTime.Today,
            RegistrationDate    = DateTime.Today,
            TaxableAmount       = 39m,
            VatAmount           = 11m,
            ExpenseTypeId       = 1,
            ChargeabilityTypeId = ChargeabilityType.Owner,
        };

        // Act
        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", expenseWithoutFiscalYear);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.FiscalYearId.Should().NotBeNull(
            "la spesa deve essere associata all'esercizio fiscale attivo anche quando non specificato esplicitamente");
        created.FiscalYearId.Should().Be(_fiscalYearId,
            "deve essere l'unico esercizio fiscale attivo del condominio");

        _expenseIds.Add(created.Id);
    }

    // ── Helpers privati ────────────────────────────────────────────────────────

    /// <summary>
    /// Costruisce un <see cref="CreateExpenseDto"/> valido per i test,
    /// con <c>FiscalYearId</c> esplicito per isolare il controllo sullo stato dell'esercizio.
    /// </summary>
    private CreateExpenseDto BuildExpense() => new()
    {
        CondominiumId       = _condominiumId,
        FiscalYearId        = _fiscalYearId,
        AccountId           = _accountUscitaId,
        MillesimalTableId   = _millesimalTableId,
        Name                = $"Spesa test {TestDataBuilder.ShortId()}",
        DocumentDate        = DateTime.Today,
        RegistrationDate    = DateTime.Today,
        TaxableAmount       = 78m,
        VatAmount           = 22m,
        ExpenseTypeId       = 1,
        ChargeabilityTypeId = ChargeabilityType.Owner,
    };

    /// <summary>
    /// Porta l'esercizio fiscale da Open fino a Closed applicando la sequenza completa:
    /// Open → Closing → (crea + approva Consuntivo) → Closed.
    /// Il Consuntivo approvato è il prerequisito obbligatorio per la chiusura
    /// introdotto dalla regola di business su <c>FiscalYearService.CloseAsync</c>.
    /// Il budget creato viene registrato in <c>_budgetIds</c> per il cleanup.
    /// </summary>
    private async Task ApproveConsuntivoAndCloseAsync()
    {
        // Crea il Consuntivo in Draft
        var (_, consuntivo) = await PostAsync<BudgetReadDto>(
            "/api/budgets", TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Consuntivo));
        _budgetIds.Add(consuntivo!.Id);

        // Approva il Consuntivo (richiede piano dei conti completo, già seedato in InitializeAsync)
        var approveResponse = await Client.PostAsJsonAsync(
            $"/api/budgets/{consuntivo.Id}/approve",
            new { NumberOfInstallments = 4, FirstDueDate = DateTime.Today });
        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "l'approvazione del Consuntivo deve riuscire come prerequisito del test");

        // Open → Closing → Closed
        await Client.PostAsJsonAsync($"/api/fiscal-years/{_fiscalYearId}/start-closing", new { notes = (string?)null });
        var closeResponse = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/close", new { notes = (string?)null });
        closeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la chiusura dell'esercizio deve riuscire come prerequisito del test");
    }

    /// <summary>
    /// Porta l'esercizio fiscale da Draft a Open tramite POST /api/fiscal-years/{id}/open.
    /// Prerequisito: l'esercizio deve essere in stato Draft.
    /// </summary>
    private async Task OpenFiscalYearAsync()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/open", new { });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la transizione Draft → Open deve riuscire come prerequisito del test");
    }

    /// <summary>
    /// Seed del piano dei conti: crea un conto per ogni tipo richiesto
    /// da <c>ApproveBudgetCommandConsumer</c> (Entrata, Uscita, Patrimoniale).
    /// Il conto Uscita viene salvato in <c>_accountUscitaId</c> per usarlo nelle spese.
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

            if (acc == null) continue;
            _accountIds.Add(acc.Id);
            if (type == ChartOfAccountsType.Uscita)
                _accountUscitaId = acc.Id;
        }
    }

    /// <summary>
    /// Seed della tabella millesimale: richiesta dal <c>CreateExpenseCommandConsumer</c>
    /// come campo obbligatorio per la ripartizione delle spese tra le unità.
    /// </summary>
    private async Task SeedMillesimalTableAsync()
    {
        var (_, table) = await PostAsync<MillesimalTableReadDto>(
            "/api/millesimal-tables",
            new CreateMillesimalTableDto
            {
                CondominiumId   = _condominiumId,
                Code            = $"MT-{TestDataBuilder.ShortId()}",
                Name            = "Tabella test",
                TotalMillesimal = 1000m,
            });
        _millesimalTableId = table!.Id;
    }
}
