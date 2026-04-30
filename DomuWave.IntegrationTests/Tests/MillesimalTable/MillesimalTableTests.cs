using System.Net;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.MillesimalTable;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.UnitMillesimal;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.MillesimalTable;

/// <summary>
/// Test di integrazione per le tabelle millesimali (api/millesimal-tables)
/// e per i valori millesimali per unità (api/unit-millesimals).
///
/// PREREQUISITI: condominio + unità immobiliare creati in InitializeAsync.
///
/// VALIDAZIONI COPERTE:
/// <list type="bullet">
///   <item>Creazione tabella → 201 Created</item>
///   <item>GET per condominio → include la tabella creata</item>
///   <item>GET per codice → restituisce la tabella corretta</item>
///   <item>Aggiornamento tabella → 200 OK</item>
///   <item>Eliminazione tabella senza millesimali → 204</item>
///   <item>Creazione millesimale unità → 201 Created</item>
///   <item>GET millesimali per tabella → include il millesimale creato</item>
///   <item>Aggiornamento millesimale → 200 OK</item>
///   <item>Eliminazione millesimale → 204</item>
///   <item>Totale millesimali calcolato sulla tabella</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class MillesimalTableTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _unitId;
    private readonly List<int> _tableIds       = [];
    private readonly List<int> _millesimalIds  = [];

    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums", TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;

        var (_, unit) = await PostAsync<RealEstateUnitReadDto>(
            "/api/real-estate-units", TestDataBuilder.RealEstateUnit(_condominiumId));
        _unitId = unit!.Id;
    }

    public override async Task DisposeAsync()
    {
        foreach (var id in _millesimalIds)
            try { await DeleteAsync($"/api/unit-millesimals/{id}"); } catch { }

        foreach (var id in _tableIds)
            try { await DeleteAsync($"/api/millesimal-tables/{id}"); } catch { }

        try { await DeleteAsync($"/api/real-estate-units/{_unitId}"); } catch { }
        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    private async Task<MillesimalTableReadDto> CreateTableAsync(
        string? code = null, string? name = null)
    {
        var dto = TestDataBuilder.MillesimalTable(_condominiumId, code, name);
        var (_, table) = await PostAsync<MillesimalTableReadDto>("/api/millesimal-tables", dto);
        _tableIds.Add(table!.Id);
        return table;
    }

    private async Task<UnitMillesimalReadDto> CreateMillesimalAsync(
        int tableId, decimal millesimal = 100m)
    {
        var dto = TestDataBuilder.UnitMillesimal(tableId, _unitId, millesimal);
        var (_, result) = await PostAsync<UnitMillesimalReadDto>("/api/unit-millesimals", dto);
        _millesimalIds.Insert(0, result!.Id);
        return result;
    }

    // ── POST /api/millesimal-tables ───────────────────────────────────────

    [Fact]
    public async Task CreateTable_ValidData_Returns201WithCode()
    {
        var dto = TestDataBuilder.MillesimalTable(_condominiumId);

        var (response, created) = await PostAsync<MillesimalTableReadDto>("/api/millesimal-tables", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Code.Should().Be(dto.Code);
        created.TotalMillesimal.Should().Be(1000m);
        created.CondominiumId.Should().Be(_condominiumId);

        _tableIds.Add(created.Id);
    }

    // ── GET /api/millesimal-tables/by-condominium/{id} ────────────────────

    [Fact]
    public async Task GetByCondominium_IncludesCreatedTable()
    {
        var table = await CreateTableAsync();

        var list = await GetAsync<IList<MillesimalTableReadDto>>(
            $"/api/millesimal-tables/by-condominium/{_condominiumId}");

        list.Should().Contain(t => t.Id == table.Id);
    }

    // ── GET /api/millesimal-tables/{id} ───────────────────────────────────

    [Fact]
    public async Task GetById_ExistingTable_ReturnsTable()
    {
        var table = await CreateTableAsync();

        var result = await GetAsync<MillesimalTableReadDto>($"/api/millesimal-tables/{table.Id}");

        result.Id.Should().Be(table.Id);
        result.Code.Should().Be(table.Code);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await Client.GetAsync("/api/millesimal-tables/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/millesimal-tables/by-condominium/{id}/code/{code} ────────

    [Fact]
    public async Task GetByCode_ExistingTable_ReturnsTable()
    {
        var uniqueCode = $"CODE-{TestDataBuilder.ShortId()}";
        var table = await CreateTableAsync(code: uniqueCode);

        var result = await GetAsync<MillesimalTableReadDto>(
            $"/api/millesimal-tables/by-condominium/{_condominiumId}/code/{uniqueCode}");

        result.Id.Should().Be(table.Id);
    }

    // ── PUT /api/millesimal-tables/{id} ───────────────────────────────────

    [Fact]
    public async Task UpdateTable_ExistingTable_Returns200WithUpdatedName()
    {
        var table = await CreateTableAsync();

        var newName   = $"Tabella Aggiornata {TestDataBuilder.ShortId()}";
        var updateDto = new UpdateMillesimalTableDto
        {
            Code            = table.Code,
            Name            = newName,
            TotalMillesimal = table.TotalMillesimal,
        };

        var (response, updated) = await PutAsync<MillesimalTableReadDto>(
            $"/api/millesimal-tables/{table.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Name.Should().Be(newName);
    }

    // ── DELETE /api/millesimal-tables/{id} ────────────────────────────────

    [Fact]
    public async Task DeleteTable_NoMillesimals_Returns204()
    {
        var table = await CreateTableAsync();
        _tableIds.Remove(table.Id);

        var response = await DeleteAsync($"/api/millesimal-tables/{table.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── POST /api/unit-millesimals ────────────────────────────────────────

    [Fact]
    public async Task CreateMillesimal_ValidData_Returns201()
    {
        var table = await CreateTableAsync();
        var dto   = TestDataBuilder.UnitMillesimal(table.Id, _unitId, millesimal: 250m);

        var (response, created) = await PostAsync<UnitMillesimalReadDto>("/api/unit-millesimals", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.MillesimalTableId.Should().Be(table.Id);
        created.UnitId.Should().Be(_unitId);
        created.Millesimal.Should().Be(250m);

        _millesimalIds.Insert(0, created.Id);
    }

    // ── GET /api/unit-millesimals/by-table/{tableId} ──────────────────────

    [Fact]
    public async Task GetMillesimalsByTable_IncludesCreatedMillesimal()
    {
        var table       = await CreateTableAsync();
        var millesimal  = await CreateMillesimalAsync(table.Id, 500m);

        var list = await GetAsync<IList<UnitMillesimalReadDto>>(
            $"/api/unit-millesimals/by-table/{table.Id}");

        list.Should().Contain(m => m.Id == millesimal.Id);
        list.Should().OnlyContain(m => m.MillesimalTableId == table.Id);
    }

    // ── PUT /api/unit-millesimals/{id} ────────────────────────────────────

    [Fact]
    public async Task UpdateMillesimal_ExistingMillesimal_Returns200WithNewValue()
    {
        var table      = await CreateTableAsync();
        var millesimal = await CreateMillesimalAsync(table.Id, 300m);

        var updateDto = new UpdateUnitMillesimalDto
        {
            Millesimal = 750m,
            Notes      = "Aggiornato",
        };

        var (response, updated) = await PutAsync<UnitMillesimalReadDto>(
            $"/api/unit-millesimals/{millesimal.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Millesimal.Should().Be(750m);
    }

    // ── DELETE /api/unit-millesimals/{id} ─────────────────────────────────

    [Fact]
    public async Task DeleteMillesimal_ExistingMillesimal_Returns204()
    {
        var table      = await CreateTableAsync();
        var millesimal = await CreateMillesimalAsync(table.Id);
        _millesimalIds.Remove(millesimal.Id);

        var response = await DeleteAsync($"/api/unit-millesimals/{millesimal.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── Multiple millesimals per table ────────────────────────────────────

    [Fact]
    public async Task GetByTable_MultipleMillesimals_ReturnsAll()
    {
        var table = await CreateTableAsync();

        var (_, unit2) = await PostAsync<RealEstateUnitReadDto>(
            "/api/real-estate-units", TestDataBuilder.RealEstateUnit(_condominiumId));

        var millesimal1 = await CreateMillesimalAsync(table.Id, 600m);

        // Crea millesimale per la seconda unità
        var dto2 = TestDataBuilder.UnitMillesimal(table.Id, unit2!.Id, 400m);
        var (_, millesimal2) = await PostAsync<UnitMillesimalReadDto>("/api/unit-millesimals", dto2);
        _millesimalIds.Insert(0, millesimal2!.Id);

        var list = await GetAsync<IList<UnitMillesimalReadDto>>(
            $"/api/unit-millesimals/by-table/{table.Id}");

        list.Select(m => m.Id).Should().Contain([millesimal1.Id, millesimal2.Id]);

        // Cleanup unità aggiuntiva
        try { await DeleteAsync($"/api/real-estate-units/{unit2.Id}"); } catch { }
    }
}
