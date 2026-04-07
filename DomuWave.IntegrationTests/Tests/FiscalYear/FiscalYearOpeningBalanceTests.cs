using System.Net;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.UnitOpeningBalance;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.FiscalYear;

/// <summary>
/// Verifica la regola di business:
/// "Il primo esercizio fiscale di un condominio può essere aperto solo dopo aver
///  inserito il saldo iniziale per tutte le unità attive."
///
/// SCENARI:
/// <list type="bullet">
///   <item>Nessuna unità → apertura consentita (nulla da bilanciare)</item>
///   <item>Unità senza saldo iniziale → 400 con messaggio esplicito</item>
///   <item>Saldo inserito solo per alcune unità → 400 (mancano N su M)</item>
///   <item>Saldo inserito per tutte le unità → 204 NoContent</item>
///   <item>Secondo esercizio → apertura consentita senza saldi manuali
///         (propagati automaticamente dal ClosingBalance precedente)</item>
/// </list>
///
/// CLEANUP: ogni test usa un condominio isolato creato e distrutto nel lifecycle.
/// </summary>
[Collection("Integration")]
public class FiscalYearOpeningBalanceTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private readonly List<int> _fiscalYearIds = [];
    private readonly List<int> _unitIds       = [];

    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums",
            TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;
    }

    public override async Task DisposeAsync()
    {
        foreach (var id in _fiscalYearIds)
            try { await DeleteAsync($"/api/fiscal-years/{id}"); } catch { }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<FiscalYearReadDto> CreateFiscalYearAsync()
    {
        var (_, fy) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years",
            TestDataBuilder.FiscalYear(_condominiumId));
        _fiscalYearIds.Add(fy!.Id);
        return fy;
    }

    private async Task<RealEstateUnitReadDto> CreateUnitAsync(string? internalNumber = null)
    {
        var (_, unit) = await PostAsync<RealEstateUnitReadDto>(
            "/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId, internalNumber));
        _unitIds.Add(unit!.Id);
        return unit;
    }

    private async Task SetOpeningBalanceAsync(int unitId, int fiscalYearId, decimal balance = 0m)
    {
        var dto = new SetUnitOpeningBalanceDto
        {
            FiscalYearId   = fiscalYearId,
            OpeningBalance = balance,
        };
        var (r, _) = await PutAsync<UnitOpeningBalanceReadDto>(
            $"/api/real-estate-units/{unitId}/opening-balance", dto);
        r.IsSuccessStatusCode.Should().BeTrue($"set opening balance unit={unitId}");
    }

    private async Task<HttpResponseMessage> OpenFiscalYearAsync(int fiscalYearId)
        => await Client.PostAsJsonAsync($"/api/fiscal-years/{fiscalYearId}/open", new { });

    // ── Test 1 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Se il condominio non ha unità immobiliari attive, il primo esercizio
    /// può essere aperto senza inserire alcun saldo iniziale.
    /// </summary>
    [Fact]
    public async Task Open_FirstFiscalYear_NoUnits_Succeeds()
    {
        var fy = await CreateFiscalYearAsync();

        var resp = await OpenFiscalYearAsync(fy.Id);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "un condominio senza unità non richiede saldi iniziali");
    }

    // ── Test 2 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Se esiste almeno un'unità attiva senza saldo iniziale, l'apertura del
    /// primo esercizio deve essere rifiutata con 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task Open_FirstFiscalYear_UnitWithoutBalance_Returns400()
    {
        var fy   = await CreateFiscalYearAsync();
        var unit = await CreateUnitAsync();

        // Nessun saldo inserito
        var resp = await OpenFiscalYearAsync(fy.Id);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "deve bloccare se manca il saldo iniziale per l'unità");

        var error = await ReadErrorAsync(resp);
        error.Should().Contain("saldo iniziale",
            "il messaggio deve spiegare che mancano i saldi iniziali");
    }

    // ── Test 3 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Se il saldo iniziale è stato inserito solo per alcune unità ma non per
    /// tutte, l'apertura deve essere bloccata con 400 e indicare quante mancano.
    /// </summary>
    [Fact]
    public async Task Open_FirstFiscalYear_PartialBalances_Returns400()
    {
        var fy    = await CreateFiscalYearAsync();
        var unit1 = await CreateUnitAsync();
        var unit2 = await CreateUnitAsync();
        var unit3 = await CreateUnitAsync();

        // Saldo inserito solo per unit1 e unit2, non per unit3
        await SetOpeningBalanceAsync(unit1.Id, fy.Id, 100m);
        await SetOpeningBalanceAsync(unit2.Id, fy.Id, -50m);

        var resp = await OpenFiscalYearAsync(fy.Id);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "manca ancora il saldo per unit3");

        var error = await ReadErrorAsync(resp);
        error.Should().Contain("1",
            "il messaggio deve indicare che manca 1 unità su 3");
    }

    // ── Test 4 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Se tutti i saldi iniziali sono stati inseriti (anche a zero),
    /// il primo esercizio deve potersi aprire con 204 NoContent.
    /// </summary>
    [Fact]
    public async Task Open_FirstFiscalYear_AllBalancesSet_Succeeds()
    {
        var fy    = await CreateFiscalYearAsync();
        var unit1 = await CreateUnitAsync();
        var unit2 = await CreateUnitAsync();

        await SetOpeningBalanceAsync(unit1.Id, fy.Id, 150m);
        await SetOpeningBalanceAsync(unit2.Id, fy.Id, 0m);   // saldo zero è comunque valido

        var resp = await OpenFiscalYearAsync(fy.Id);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "tutti i saldi iniziali sono stati inseriti");
    }

    // ── Test 5 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Il vincolo si applica solo al PRIMO esercizio.
    /// Per un secondo esercizio (con esercizio precedente chiuso), l'apertura
    /// deve riuscire senza richiedere saldi manuali (sono propagati dal ClosingBalance).
    /// </summary>
    [Fact]
    public async Task Open_SecondFiscalYear_WithoutManualBalances_Succeeds()
    {
        // Crea unità
        var unit = await CreateUnitAsync();

        // Crea e apre il primo esercizio (con saldo)
        var fy1 = await CreateFiscalYearAsync();
        await SetOpeningBalanceAsync(unit.Id, fy1.Id, 0m);
        var openResp = await OpenFiscalYearAsync(fy1.Id);
        openResp.StatusCode.Should().Be(HttpStatusCode.NoContent, "primo esercizio");

        // Chiude il primo esercizio (necessario affinché il secondo lo trovi come precedente)
        await Client.PostAsJsonAsync($"/api/fiscal-years/{fy1.Id}/start-closing",
            new { notes = "chiusura test" });
        await Client.PostAsJsonAsync($"/api/fiscal-years/{fy1.Id}/close",
            new { notes = "chiusura test" });

        // Crea il secondo esercizio nell'anno successivo
        var (_, fy2) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years",
            new Services.Dto.Contabilita.FiscalYear.FiscalYearCreateDto
            {
                CondominiumId = _condominiumId,
                Code          = $"FY2-{TestDataBuilder.ShortId()}",
                Description   = "Secondo esercizio",
                StartDate     = new DateTime(DateTime.UtcNow.Year + 1, 1, 1),
                EndDate       = new DateTime(DateTime.UtcNow.Year + 1, 12, 31),
            });
        _fiscalYearIds.Add(fy2!.Id);

        // Apre il secondo esercizio senza inserire saldi manuali
        var resp = await OpenFiscalYearAsync(fy2.Id);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "il secondo esercizio non richiede saldi manuali: vengono propagati automaticamente");
    }
}
