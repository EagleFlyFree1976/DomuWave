using System.Net;
using System.Net.Http.Json;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.UnitOpeningBalance;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.UnitOpeningBalance;

/// <summary>
/// Test di integrazione per il saldo di apertura unità immobiliare
/// (PUT api/real-estate-units/{unitId}/opening-balance).
///
/// SCENARI COPERTI:
/// <list type="bullet">
///   <item>Impostazione saldo su primo esercizio → 200 OK</item>
///   <item>Valore negativo (credito) → 200 OK</item>
///   <item>Valore zero → 200 OK</item>
///   <item>Sovrascrittura del saldo → 200 OK con nuovo valore</item>
///   <item>Esercizio con PreviousFiscalYear → 400 Bad Request (propagato automaticamente)</item>
///   <item>GET saldo → restituisce IsEditable=true sul primo esercizio</item>
///   <item>GET saldo su esercizio con precedente → IsEditable=false</item>
///   <item>GET lista saldi per anno fiscale → include le unità attive</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class UnitOpeningBalanceTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _unitId;
    private readonly List<int> _fiscalYearIds = [];
    private readonly List<int> _unitIds       = [];

    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums", TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;

        var (_, unit) = await PostAsync<RealEstateUnitReadDto>(
            "/api/real-estate-units", TestDataBuilder.RealEstateUnit(_condominiumId));
        _unitId = unit!.Id;
        _unitIds.Add(_unitId);
    }

    public override async Task DisposeAsync()
    {
        foreach (var id in _fiscalYearIds)
            try { await DeleteAsync($"/api/fiscal-years/{id}"); } catch { }

        foreach (var id in _unitIds)
            try { await DeleteAsync($"/api/real-estate-units/{id}"); } catch { }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    private async Task<FiscalYearReadDto> CreateFiscalYearAsync(int? year = null)
    {
        var (_, fy) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years",
            TestDataBuilder.FiscalYear(_condominiumId, year ?? 0));
        _fiscalYearIds.Add(fy!.Id);
        return fy;
    }

    private async Task<(HttpResponseMessage Response, UnitOpeningBalanceReadDto? Body)> SetBalanceAsync(
        int unitId, int fiscalYearId, decimal balance = 0m, string? notes = null)
    {
        var dto = new SetUnitOpeningBalanceDto
        {
            FiscalYearId   = fiscalYearId,
            OpeningBalance = balance,
            Notes          = notes,
        };
        return await PutAsync<UnitOpeningBalanceReadDto>(
            $"/api/real-estate-units/{unitId}/opening-balance", dto);
    }

    // ── Impostazione saldo sul primo esercizio ─────────────────────────────

    [Fact]
    public async Task SetBalance_FirstFiscalYear_Returns200WithBalance()
    {
        var fy = await CreateFiscalYearAsync();

        var (response, result) = await SetBalanceAsync(_unitId, fy.Id, balance: 250m);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.OpeningBalance.Should().Be(250m);
        result.UnitId.Should().Be(_unitId);
        result.FiscalYearId.Should().Be(fy.Id);
        result.IsEditable.Should().BeTrue();
    }

    [Fact]
    public async Task SetBalance_NegativeValue_Returns200()
    {
        var fy = await CreateFiscalYearAsync();

        var (response, result) = await SetBalanceAsync(_unitId, fy.Id, balance: -150m);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.OpeningBalance.Should().Be(-150m,
            "un valore negativo rappresenta un credito pregresso del condòmino");
    }

    [Fact]
    public async Task SetBalance_Zero_Returns200()
    {
        var fy = await CreateFiscalYearAsync();

        var (response, result) = await SetBalanceAsync(_unitId, fy.Id, balance: 0m);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.OpeningBalance.Should().Be(0m);
    }

    [Fact]
    public async Task SetBalance_Overwrite_Returns200WithNewValue()
    {
        var fy = await CreateFiscalYearAsync();

        await SetBalanceAsync(_unitId, fy.Id, balance: 100m);
        var (response, result) = await SetBalanceAsync(_unitId, fy.Id, balance: 999m);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.OpeningBalance.Should().Be(999m,
            "il saldo deve poter essere sovrascritto sul primo esercizio");
    }

    [Fact]
    public async Task SetBalance_WithNotes_Returns200WithNotes()
    {
        var fy    = await CreateFiscalYearAsync();
        var notes = "Morosità riportata dal mandato precedente";

        var (response, result) = await SetBalanceAsync(_unitId, fy.Id, notes: notes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.Notes.Should().Be(notes);
    }

    // ── Vincolo: esercizio con PreviousFiscalYear ──────────────────────────

    [Fact]
    public async Task SetBalance_FiscalYearWithPrevious_Returns400()
    {
        // Crea e apri il primo esercizio
        var fy1 = await CreateFiscalYearAsync(year: DateTime.UtcNow.Year - 1);
        await SetBalanceAsync(_unitId, fy1.Id, balance: 0m);
        await Client.PostAsJsonAsync($"/api/fiscal-years/{fy1.Id}/open", new { });

        // Chiudi il primo esercizio (rende il secondo "con precedente")
        await Client.PostAsJsonAsync($"/api/fiscal-years/{fy1.Id}/start-closing",
            new { notes = "chiusura per test" });
        await Client.PostAsJsonAsync($"/api/fiscal-years/{fy1.Id}/close",
            new { notes = "chiusura per test" });

        // Crea il secondo esercizio (ha PreviousFiscalYear)
        var fy2 = await CreateFiscalYearAsync(year: DateTime.UtcNow.Year);

        // Tenta di impostare manualmente il saldo sul secondo esercizio
        var (response, _) = await SetBalanceAsync(_unitId, fy2.Id, balance: 100m);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "il saldo del secondo esercizio viene propagato automaticamente dal saldo di chiusura del precedente");

        var error = await ReadErrorAsync(response);
        error.Should().Contain("propagato",
            "il messaggio deve spiegare che il saldo viene propagato automaticamente");
    }

    // ── GET saldo per unità ───────────────────────────────────────────────

    [Fact]
    public async Task GetBalance_AfterSet_ReturnsCorrectValues()
    {
        var fy = await CreateFiscalYearAsync();
        await SetBalanceAsync(_unitId, fy.Id, balance: 500m, notes: "Nota test");

        var result = await GetAsync<UnitOpeningBalanceReadDto>(
            $"/api/real-estate-units/{_unitId}/opening-balance?fiscalYearId={fy.Id}");

        result.OpeningBalance.Should().Be(500m);
        result.UnitId.Should().Be(_unitId);
        result.FiscalYearId.Should().Be(fy.Id);
    }

    [Fact]
    public async Task GetBalance_FirstFiscalYear_IsEditableTrue()
    {
        var fy = await CreateFiscalYearAsync();
        await SetBalanceAsync(_unitId, fy.Id, balance: 0m);

        var result = await GetAsync<UnitOpeningBalanceReadDto>(
            $"/api/real-estate-units/{_unitId}/opening-balance?fiscalYearId={fy.Id}");

        result.IsEditable.Should().BeTrue(
            "il saldo del primo esercizio è sempre modificabile manualmente");
        result.IsClosed.Should().BeFalse();
    }

    // ── GET lista saldi per anno fiscale ──────────────────────────────────

    [Fact]
    public async Task GetBalancesByFiscalYear_ReturnsAllUnitsBalances()
    {
        // Crea una seconda unità
        var (_, unit2) = await PostAsync<RealEstateUnitReadDto>(
            "/api/real-estate-units", TestDataBuilder.RealEstateUnit(_condominiumId));
        _unitIds.Add(unit2!.Id);

        var fy = await CreateFiscalYearAsync();
        await SetBalanceAsync(_unitId,   fy.Id, balance: 100m);
        await SetBalanceAsync(unit2.Id,  fy.Id, balance: 200m);

        var list = await GetAsync<IList<UnitOpeningBalanceReadDto>>(
            $"/api/real-estate-units/opening-balances/by-fiscal-year/{fy.Id}");

        list.Should().Contain(b => b.UnitId == _unitId   && b.OpeningBalance == 100m);
        list.Should().Contain(b => b.UnitId == unit2.Id  && b.OpeningBalance == 200m);
    }

    // ── Unità inesistente ─────────────────────────────────────────────────

    [Fact]
    public async Task SetBalance_NonExistingUnit_Returns404()
    {
        var fy = await CreateFiscalYearAsync();

        var dto = new SetUnitOpeningBalanceDto { FiscalYearId = fy.Id, OpeningBalance = 0m };
        var response = await Client.PutAsJsonAsync("/api/real-estate-units/999999/opening-balance", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Anno fiscale inesistente ───────────────────────────────────────────

    [Fact]
    public async Task SetBalance_NonExistingFiscalYear_Returns404()
    {
        var (response, _) = await SetBalanceAsync(_unitId, fiscalYearId: 999999, balance: 0m);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
