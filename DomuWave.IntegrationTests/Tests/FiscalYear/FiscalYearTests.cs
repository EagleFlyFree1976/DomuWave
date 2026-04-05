using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using FluentAssertions;
using System.Net;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.FiscalYear;

/// <summary>
/// Test di integrazione per gli endpoint di <c>/api/fiscal-years</c>.
///
/// PREREQUISITI: ogni esecuzione della classe crea un condominio fresco in
/// <see cref="InitializeAsync"/> e lo elimina in <see cref="DisposeAsync"/>.
/// Gli anni fiscali creati vengono registrati in <c>_fiscalYearIds</c> per il cleanup.
///
/// COPERTURA:
/// <list type="bullet">
///   <item>POST /api/fiscal-years → 201 Created (valido)</item>
///   <item>POST /api/fiscal-years → 400 Bad Request (EndDate &lt; StartDate, codice duplicato)</item>
///   <item>GET  /api/fiscal-years/by-condominium/{id} → lista include l'anno creato</item>
///   <item>GET  /api/fiscal-years/{id} → 200 OK | 404 Not Found</item>
///   <item>DELETE /api/fiscal-years/{id} → 204 NoContent</item>
/// </list>
///
/// REGOLA DI BUSINESS: un anno fiscale con <c>EndDate &lt; StartDate</c> è invalido
/// e deve essere rifiutato con 400. Un codice duplicato nello stesso condominio è
/// anch'esso invalido (unicità per condominio + codice).
/// </summary>
[Collection("Integration")]
public class FiscalYearTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    /// <summary>Id del condominio creato in <see cref="InitializeAsync"/> per questa classe di test.</summary>
    private int _condominiumId;

    /// <summary>Id degli anni fiscali creati durante i test, per il cleanup in <see cref="DisposeAsync"/>.</summary>
    private readonly List<int> _fiscalYearIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────

    /// <summary>
    /// Setup: crea un condominio fresco da usare come contesto per tutti i test
    /// di questa classe. Il condominio viene eliminato in <see cref="DisposeAsync"/>.
    /// </summary>
    public override async Task InitializeAsync()
    {
        // Crea un condominio con codice univoco per evitare conflitti con altri test paralleli
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums",
            TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;
    }

    /// <summary>
    /// Teardown best-effort: elimina prima tutti gli anni fiscali creati,
    /// poi il condominio di contesto.
    /// L'ordine è importante: il condominio non può essere eliminato se ha anni fiscali attivi.
    /// </summary>
    public override async Task DisposeAsync()
    {
        foreach (var id in _fiscalYearIds)
        {
            try { await DeleteAsync($"/api/fiscal-years/{id}"); }
            catch { /* ignora */ }
        }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); }
        catch { /* ignora */ }

        await base.DisposeAsync();
    }

    // ── POST /api/fiscal-years ─────────────────────────────────────────────

    /// <summary>
    /// Verifica che la creazione di un anno fiscale valido (date corrette, codice univoco)
    /// restituisca 201 Created con il DTO corretto: Id generato, codice e
    /// CondominiumId preservati.
    /// </summary>
    [Fact]
    public async Task Create_ValidFiscalYear_Returns201()
    {
        // Arrange
        var dto = TestDataBuilder.FiscalYear(_condominiumId);

        // Act
        var (response, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Code.Should().Be(dto.Code);
        created.CondominiumId.Should().Be(_condominiumId);

        _fiscalYearIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che la creazione con EndDate precedente a StartDate restituisca
    /// 400 Bad Request. Un anno fiscale con date invertite è logicamente invalido
    /// (es. StartDate = 2024-01-01, EndDate = 2023-12-31).
    /// </summary>
    [Fact]
    public async Task Create_EndDateBeforeStartDate_Returns400()
    {
        // Arrange: EndDate viene impostata un giorno prima di StartDate (invalido)
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        dto.EndDate = dto.StartDate.AddDays(-1); // data fine < data inizio → invalido

        // Act
        var (response, _) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifica che la creazione di un secondo anno fiscale con lo stesso codice
    /// nello stesso condominio restituisca 400 Bad Request.
    /// Il codice è univoco per condominio (non a livello globale).
    /// </summary>
    [Fact]
    public async Task Create_DuplicateCode_Returns400()
    {
        // Arrange: crea il primo anno fiscale
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        var (_, first) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);
        _fiscalYearIds.Add(first!.Id);

        // Act: secondo tentativo con lo stesso dto (stesso codice) → deve fallire
        var (response, _) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/fiscal-years/by-condominium/{id} ──────────────────────────

    /// <summary>
    /// Verifica che la GET per condominio restituisca una lista che include l'anno
    /// fiscale appena creato. Può restituire anche anni precedenti se il DB non è pulito.
    /// </summary>
    [Fact]
    public async Task GetByCondominium_ReturnsListIncludingCreated()
    {
        // Arrange: crea un anno fiscale e registra il suo Id
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        var (_, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);
        _fiscalYearIds.Add(created!.Id);

        // Act
        var list = await GetAsync<IList<FiscalYearReadDto>>(
            $"/api/fiscal-years/by-condominium/{_condominiumId}");

        // Assert: la lista deve contenere almeno l'anno appena creato
        list.Should().Contain(fy => fy.Id == created.Id);
    }

    // ── GET /api/fiscal-years/{id} ─────────────────────────────────────────

    /// <summary>
    /// Verifica che la GET per id di un anno fiscale esistente restituisca 200 OK
    /// con il DTO corretto (stesso Id e CondominiumId).
    /// </summary>
    [Fact]
    public async Task GetById_ExistingFiscalYear_ReturnsDto()
    {
        // Arrange
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        var (_, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);
        _fiscalYearIds.Add(created!.Id);

        // Act
        var result = await GetAsync<FiscalYearReadDto>($"/api/fiscal-years/{created.Id}");

        // Assert
        result.Id.Should().Be(created.Id);
        result.CondominiumId.Should().Be(_condominiumId);
    }

    /// <summary>
    /// Verifica che la GET per un id inesistente (999999) restituisca 404 Not Found.
    /// </summary>
    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await Client.GetAsync("/api/fiscal-years/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/fiscal-years/{id} ──────────────────────────────────────

    /// <summary>
    /// Verifica che la DELETE su un anno fiscale esistente (senza budget e senza
    /// dati contabili associati) restituisca 204 NoContent.
    /// Non registra l'Id in <c>_fiscalYearIds</c> perché il test stesso lo elimina.
    /// </summary>
    [Fact]
    public async Task Delete_ExistingFiscalYear_Returns204()
    {
        // Arrange
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        var (_, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        // Act — elimina direttamente (non aggiunto a _fiscalYearIds)
        var response = await DeleteAsync($"/api/fiscal-years/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
