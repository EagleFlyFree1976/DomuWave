using System.Net;
using System.Net.Http.Json;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
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
/// Test di integrazione per la regola di business sulla coerenza della data di
/// registrazione rispetto al periodo dell'esercizio fiscale selezionato.
///
/// REGOLA DI BUSINESS:
/// <list type="bullet">
///   <item>
///     Esercizio in stato <b>Open</b>: la <c>RegistrationDate</c> deve essere compresa
///     nel periodo [<c>StartDate</c>, <c>EndDate</c>] dell'esercizio.
///     Una data fuori periodo restituisce <b>400 Bad Request</b>.
///   </item>
///   <item>
///     Esercizio in stato <b>Closing</b>: la registrazione è consentita anche con una
///     data fuori periodo (rettifiche tardive in fase di chiusura). Restituisce <b>201 Created</b>.
///   </item>
/// </list>
///
/// PREREQUISITI CREATI IN <see cref="InitializeAsync"/>:
/// <list type="number">
///   <item>Condominio</item>
///   <item>Anno fiscale con periodo fisso nell'anno corrente (1 gen – 31 dic)</item>
///   <item>Piano dei conti: 1 conto per tipo (Entrata, Uscita, Patrimoniale)</item>
///   <item>Tabella millesimale</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class ExpenseRegistrationDateTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _fiscalYearId;
    private int _accountUscitaId;
    private int _millesimalTableId;

    private DateTime _fyStart;
    private DateTime _fyEnd;

    private readonly List<int>  _accountIds = [];
    private readonly List<long> _expenseIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums", TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;

        // Esercizio con periodo determinato: serve per costruire date dentro/fuori range
        var fyDto = TestDataBuilder.FiscalYear(_condominiumId);
        _fyStart = fyDto.StartDate;
        _fyEnd   = fyDto.EndDate;

        var (_, fy) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", fyDto);
        _fiscalYearId = fy!.Id;

        await SeedChartOfAccountsAsync();
        await SeedMillesimalTableAsync();
    }

    public override async Task DisposeAsync()
    {
        foreach (var id in _expenseIds)
            try { await DeleteAsync($"/api/expenses/{id}"); } catch { }

        foreach (var id in _accountIds)
            try { await DeleteAsync($"/api/chart-of-accounts/{id}"); } catch { }

        try { await DeleteAsync($"/api/millesimal-tables/{_millesimalTableId}"); } catch { }
        try { await DeleteAsync($"/api/fiscal-years/{_fiscalYearId}"); }          catch { }
        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); }          catch { }

        await base.DisposeAsync();
    }

    // ── Esercizio Open ─────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che una spesa con data di registrazione all'interno del periodo
    /// dell'esercizio Open venga accettata con 201 Created.
    /// Scenario: data = StartDate (bordo inferiore).
    /// </summary>
    [Fact]
    public async Task CreateExpense_Open_RegistrationDateOnStartDate_Returns201()
    {
        await OpenFiscalYearAsync();

        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense(registrationDate: _fyStart));

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "la data coincide con StartDate dell'esercizio: deve essere accettata");
        if (created != null) _expenseIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che una spesa con data di registrazione all'interno del periodo
    /// dell'esercizio Open venga accettata con 201 Created.
    /// Scenario: data = EndDate (bordo superiore).
    /// </summary>
    [Fact]
    public async Task CreateExpense_Open_RegistrationDateOnEndDate_Returns201()
    {
        await OpenFiscalYearAsync();

        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense(registrationDate: _fyEnd));

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "la data coincide con EndDate dell'esercizio: deve essere accettata");
        if (created != null) _expenseIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che una spesa con data di registrazione precedente a StartDate
    /// dell'esercizio Open venga rifiutata con 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task CreateExpense_Open_RegistrationDateBeforeStart_Returns400()
    {
        await OpenFiscalYearAsync();

        var dateBeforeStart = _fyStart.AddDays(-1);
        var (response, _) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense(registrationDate: dateBeforeStart));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "la data è precedente a StartDate: deve essere rifiutata su un esercizio Open");

        var error = await ReadErrorAsync(response);
        error.Should().Contain("data di registrazione",
            "il messaggio deve menzionare la data di registrazione non valida");
    }

    /// <summary>
    /// Verifica che una spesa con data di registrazione successiva a EndDate
    /// dell'esercizio Open venga rifiutata con 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task CreateExpense_Open_RegistrationDateAfterEnd_Returns400()
    {
        await OpenFiscalYearAsync();

        var dateAfterEnd = _fyEnd.AddDays(1);
        var (response, _) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense(registrationDate: dateAfterEnd));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "la data è successiva a EndDate: deve essere rifiutata su un esercizio Open");

        var error = await ReadErrorAsync(response);
        error.Should().Contain("data di registrazione",
            "il messaggio deve menzionare la data di registrazione non valida");
    }

    // ── Esercizio Closing ──────────────────────────────────────────────────────

    /// <summary>
    /// Verifica che su un esercizio in stato Closing una spesa con data di registrazione
    /// all'interno del periodo sia accettata normalmente (201 Created).
    /// </summary>
    [Fact]
    public async Task CreateExpense_Closing_RegistrationDateInRange_Returns201()
    {
        await OpenFiscalYearAsync();
        await StartClosingAsync();

        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense(registrationDate: _fyStart));

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "data nel periodo: deve essere accettata anche su un esercizio Closing");
        if (created != null) _expenseIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che su un esercizio in stato Closing una spesa con data di registrazione
    /// <b>precedente</b> a StartDate sia comunque accettata (201 Created).
    /// La fase di chiusura permette rettifiche tardive fuori dal periodo ufficiale.
    /// </summary>
    [Fact]
    public async Task CreateExpense_Closing_RegistrationDateBeforeStart_Returns201()
    {
        await OpenFiscalYearAsync();
        await StartClosingAsync();

        var dateBeforeStart = _fyStart.AddDays(-1);
        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense(registrationDate: dateBeforeStart));

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "su un esercizio Closing la data fuori periodo (prima) deve essere accettata");
        if (created != null) _expenseIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che su un esercizio in stato Closing una spesa con data di registrazione
    /// <b>successiva</b> a EndDate sia comunque accettata (201 Created).
    /// La fase di chiusura permette rettifiche tardive fuori dal periodo ufficiale.
    /// </summary>
    [Fact]
    public async Task CreateExpense_Closing_RegistrationDateAfterEnd_Returns201()
    {
        await OpenFiscalYearAsync();
        await StartClosingAsync();

        var dateAfterEnd = _fyEnd.AddDays(1);
        var (response, created) = await PostAsync<ExpenseReadDto>(
            "/api/expenses", BuildExpense(registrationDate: dateAfterEnd));

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "su un esercizio Closing la data fuori periodo (dopo) deve essere accettata");
        if (created != null) _expenseIds.Add(created.Id);
    }

    // ── Helpers privati ────────────────────────────────────────────────────────

    private CreateExpenseDto BuildExpense(DateTime? registrationDate = null) => new()
    {
        CondominiumId       = _condominiumId,
        FiscalYearId        = _fiscalYearId,
        AccountId           = _accountUscitaId,
        MillesimalTableId   = _millesimalTableId,
        Name                = $"Spesa test {TestDataBuilder.ShortId()}",
        DocumentDate        = DateTime.Today,
        RegistrationDate    = registrationDate ?? DateTime.Today,
        TaxableAmount       = 78m,
        VatAmount           = 22m,
        ExpenseTypeId       = 1,
        ChargeabilityTypeId = ChargeabilityType.Owner,
    };

    private async Task OpenFiscalYearAsync()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/open", new { });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la transizione Draft → Open deve riuscire come prerequisito del test");
    }

    private async Task StartClosingAsync()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/fiscal-years/{_fiscalYearId}/start-closing", new { notes = (string?)null });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "la transizione Open → Closing deve riuscire come prerequisito del test");
    }

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
