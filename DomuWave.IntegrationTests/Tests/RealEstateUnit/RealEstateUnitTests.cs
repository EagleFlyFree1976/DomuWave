using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.RealEstateUnit;
using FluentAssertions;
using System.Net;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.RealEstateUnit;

/// <summary>
/// Test di integrazione per gli endpoint CRUD di <c>/api/real-estate-units</c>.
///
/// PREREQUISITI: ogni esecuzione della classe crea un condominio fresco in
/// <see cref="InitializeAsync"/> e lo elimina in <see cref="DisposeAsync"/> dopo aver
/// eliminato tutte le unità create. Le unità vengono registrate in <c>_unitIds</c>.
///
/// COPERTURA:
/// <list type="bullet">
///   <item>POST /api/real-estate-units → 201 Created (payload valido)</item>
///   <item>POST /api/real-estate-units → 400/404 (condominiumId = 0)</item>
///   <item>GET  /api/real-estate-units/by-condominium/{id} → lista con entrambe le unità create</item>
///   <item>GET  /api/real-estate-units/{id} → 200 OK | 404 Not Found</item>
///   <item>PUT  /api/real-estate-units/{id} → 200 OK con campi aggiornati</item>
///   <item>DELETE /api/real-estate-units/{id} → 204 NoContent</item>
/// </list>
///
/// DIPENDENZE: ogni unità immobiliare richiede un condominio valido come FK obbligatoria.
/// </summary>
[Collection("Integration")]
public class RealEstateUnitTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    /// <summary>Id del condominio di contesto creato in <see cref="InitializeAsync"/>.</summary>
    private int _condominiumId;

    /// <summary>Id delle unità create durante i test, per il cleanup in <see cref="DisposeAsync"/>.</summary>
    private readonly List<int> _unitIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────

    /// <summary>
    /// Setup: crea un condominio fresco con codice univoco da usare come contesto
    /// per tutte le unità immobiliari di questa classe di test.
    /// </summary>
    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums",
            TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;
    }

    /// <summary>
    /// Teardown best-effort: elimina prima le unità immobiliari (FK verso il condominio),
    /// poi il condominio stesso. L'ordine è necessario per rispettare i vincoli di FK.
    /// </summary>
    public override async Task DisposeAsync()
    {
        foreach (var id in _unitIds)
        {
            try { await DeleteAsync($"/api/real-estate-units/{id}"); }
            catch { /* ignora */ }
        }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); }
        catch { /* ignora */ }

        await base.DisposeAsync();
    }

    // ── POST /api/real-estate-units ────────────────────────────────────────

    /// <summary>
    /// Verifica che la creazione di un'unità immobiliare con payload valido
    /// (tipo "Appartamento", scala A, piano 1, 80 mq) restituisca 201 Created
    /// con Id generato e CondominiumId corretto.
    /// </summary>
    [Fact]
    public async Task Create_ValidUnit_Returns201()
    {
        // Arrange
        var dto = TestDataBuilder.RealEstateUnit(_condominiumId);

        // Act
        var (response, created) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.CondominiumId.Should().Be(_condominiumId); // FK verificata

        _unitIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che la creazione con <c>CondominiumId = 0</c> (FK invalida) restituisca
    /// 400 Bad Request o 404 Not Found. Il comportamento dipende da dove avviene la
    /// validazione: se a livello di model validation (400) o nel consumer (404 se
    /// il condominio 0 non esiste nel DB).
    /// </summary>
    [Fact]
    public async Task Create_MissingCondominiumId_Returns400Or404()
    {
        // Arrange: CondominiumId = 0 è invalido (nessun condominio con Id = 0 esiste)
        var dto = TestDataBuilder.RealEstateUnit(condominiumId: 0);

        // Act
        var (response, _) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units", dto);

        // Assert: accetta sia 400 (validation) che 404 (not found nel consumer)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    // ── GET /api/real-estate-units/by-condominium/{id} ────────────────────

    /// <summary>
    /// Verifica che la GET per condominio restituisca tutte le unità create.
    /// Crea 2 unità e verifica che entrambe compaiano nella lista filtrata per condominio.
    /// </summary>
    [Fact]
    public async Task GetByCondominium_ReturnsAllUnitsForCondominium()
    {
        // Arrange: crea 2 unità con numeri interni univoci
        var (_, u1) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));
        var (_, u2) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));

        _unitIds.AddRange([u1!.Id, u2!.Id]);

        // Act
        var list = await GetAsync<IList<RealEstateUnitReadDto>>(
            $"/api/real-estate-units/by-condominium/{_condominiumId}");

        // Assert: la lista deve contenere entrambe le unità create
        list.Should().Contain(u => u.Id == u1.Id)
            .And.Contain(u => u.Id == u2.Id);
    }

    // ── GET /api/real-estate-units/{id} ───────────────────────────────────

    /// <summary>
    /// Verifica che la GET per id di un'unità esistente restituisca 200 OK con il
    /// DTO corretto (stesso Id dell'unità creata).
    /// </summary>
    [Fact]
    public async Task GetById_Existing_ReturnsDto()
    {
        // Arrange
        var (_, created) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));
        _unitIds.Add(created!.Id);

        // Act
        var result = await GetAsync<RealEstateUnitReadDto>($"/api/real-estate-units/{created.Id}");

        // Assert
        result.Id.Should().Be(created.Id);
    }

    /// <summary>
    /// Verifica che la GET per un id inesistente (999999) restituisca 404 Not Found.
    /// </summary>
    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await Client.GetAsync("/api/real-estate-units/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/real-estate-units/{id} ───────────────────────────────────

    /// <summary>
    /// Verifica che la PUT su un'unità esistente restituisca 200 OK con i campi
    /// aggiornati. Il test aggiorna Note, AreaSqm e IsActive, e verifica che
    /// <c>AreaSqm</c> rifletta il nuovo valore (90 mq).
    /// </summary>
    [Fact]
    public async Task Update_ExistingUnit_ReturnsUpdated()
    {
        // Arrange: crea l'unità da aggiornare
        var (_, created) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));
        _unitIds.Add(created!.Id);

        var updateDto = new UpdateRealEstateUnitDto
        {
            Notes    = "Nota aggiornata",
            AreaSqm  = 90m,   // era 80 mq, aggiornato a 90 mq
            IsActive = true,
        };

        // Act
        var (response, updated) = await PutAsync<RealEstateUnitReadDto>(
            $"/api/real-estate-units/{created.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.AreaSqm.Should().Be(updateDto.AreaSqm); // verifica che la superficie sia aggiornata
    }

    // ── DELETE /api/real-estate-units/{id} ────────────────────────────────

    /// <summary>
    /// Verifica che la DELETE su un'unità esistente restituisca 204 NoContent.
    /// L'unità non viene registrata in <c>_unitIds</c> perché il test stesso la elimina.
    /// </summary>
    [Fact]
    public async Task Delete_ExistingUnit_Returns204()
    {
        // Arrange
        var (_, created) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));

        // Act — elimina direttamente (non aggiunta a _unitIds)
        var response = await DeleteAsync($"/api/real-estate-units/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
