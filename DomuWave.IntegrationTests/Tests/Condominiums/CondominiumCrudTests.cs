using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using FluentAssertions;
using System.Net;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.Condominiums;

/// <summary>
/// Test di integrazione per gli endpoint CRUD di <c>/api/condominiums</c>.
///
/// COPERTURA:
/// <list type="bullet">
///   <item>POST /api/condominiums → 201 Created (payload valido)</item>
///   <item>POST /api/condominiums → 400 Bad Request (codice vuoto, codice duplicato)</item>
///   <item>GET  /api/condominiums → 200 OK (lista)</item>
///   <item>GET  /api/condominiums/{id} → 200 OK | 404 Not Found</item>
///   <item>GET  /api/condominiums/by-code/{code} → 200 OK</item>
///   <item>PUT  /api/condominiums/{id} → 200 OK | 404 Not Found</item>
///   <item>DELETE /api/condominiums/{id} → 204 NoContent | 404 Not Found + verifica GET successivo</item>
/// </list>
///
/// ISOLAMENTO: ogni test crea i propri dati (codice univoco via <see cref="TestDataBuilder.ShortId"/>)
/// e li registra in <c>_createdIds</c> per la pulizia in <see cref="DisposeAsync"/>.
/// I test possono essere eseguiti in qualsiasi ordine contro un DB condiviso senza interferire.
/// </summary>
[Collection("Integration")]
public class CondominiumCrudTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    /// <summary>
    /// Id dei condomìni creati durante questa esecuzione di test.
    /// Usato in <see cref="DisposeAsync"/> per il cleanup best-effort.
    /// </summary>
    private readonly List<int> _createdIds = [];

    // ── POST /api/condominiums ─────────────────────────────────────────────

    /// <summary>
    /// Verifica che la creazione di un condominio con payload valido restituisca
    /// 201 Created con il DTO completo, inclusi Id generato, codice e nome corretti.
    /// </summary>
    [Fact]
    public async Task Create_ValidPayload_Returns201WithDto()
    {
        // Arrange
        var dto = TestDataBuilder.Condominium();

        // Act
        var (response, created) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);                // Id assegnato da hilo
        created.Code.Should().Be(dto.Code);                   // codice preservato
        created.Name.Should().Be(dto.Name);                   // nome preservato

        _createdIds.Add(created.Id);
    }

    /// <summary>
    /// Verifica che la creazione con codice vuoto ("") restituisca 400 Bad Request.
    /// Il codice è un campo obbligatorio e univoco per tenant.
    /// </summary>
    [Fact]
    public async Task Create_EmptyCode_Returns400()
    {
        // Arrange
        var dto = TestDataBuilder.Condominium(code: "");

        // Act
        var (response, _) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifica che il tentativo di creare un secondo condominio con lo stesso codice
    /// del primo restituisca 400 Bad Request con un messaggio di errore che contiene il
    /// codice duplicato. Garantisce l'unicità del codice per tenant a livello di API.
    /// </summary>
    [Fact]
    public async Task Create_DuplicateCode_Returns400()
    {
        // Arrange — crea il primo condominio con un codice univoco
        var dto = TestDataBuilder.Condominium();
        var (_, first) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);
        first.Should().NotBeNull();
        _createdIds.Add(first!.Id);

        // Act — secondo condominio con lo stesso codice deve essere rifiutato
        var (response, _) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await ReadErrorAsync(response);
        error.Should().Contain(dto.Code); // il messaggio di errore deve citare il codice duplicato
    }

    // ── GET /api/condominiums ──────────────────────────────────────────────

    /// <summary>
    /// Verifica che la GET sulla lista dei condomìni restituisca 200 OK con una
    /// risposta non null (la lista può essere vuota se il DB di test è pulito).
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var result = await GetAsync<IList<CondominiumReadDto>>("/api/condominiums");
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Verifica che la GET per id di un condominio esistente restituisca 200 OK
    /// con il DTO corretto (stesso Id e codice del condominio creato).
    /// </summary>
    [Fact]
    public async Task GetById_ExistingId_ReturnsCondominium()
    {
        // Arrange — crea un condominio e recupera il suo Id
        var dto = TestDataBuilder.Condominium();
        var (_, created) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);
        _createdIds.Add(created!.Id);

        // Act
        var result = await GetAsync<CondominiumReadDto>($"/api/condominiums/{created.Id}");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(created.Id);
        result.Code.Should().Be(dto.Code);
    }

    /// <summary>
    /// Verifica che la GET per un id inesistente (999999) restituisca 404 Not Found.
    /// Garantisce che il controller non restituisca una risposta vuota o un errore 500.
    /// </summary>
    [Fact]
    public async Task GetById_NonExistingId_Returns404()
    {
        var response = await Client.GetAsync("/api/condominiums/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifica che la GET per codice restituisca il condominio corrispondente.
    /// L'endpoint /by-code/{code} è usato per lookup rapido da sistemi esterni.
    /// </summary>
    [Fact]
    public async Task GetByCode_ExistingCode_ReturnsCondominium()
    {
        // Arrange
        var dto = TestDataBuilder.Condominium();
        var (_, created) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);
        _createdIds.Add(created!.Id);

        // Act
        var result = await GetAsync<CondominiumReadDto>($"/api/condominiums/by-code/{dto.Code}");

        // Assert
        result.Id.Should().Be(created.Id);
    }

    // ── PUT /api/condominiums/{id} ─────────────────────────────────────────

    /// <summary>
    /// Verifica che la PUT su un condominio esistente restituisca 200 OK con il DTO
    /// aggiornato. Il test aggiorna solo il nome (usando un suffisso univoco) e verifica
    /// che il nuovo nome sia presente nella risposta.
    /// </summary>
    [Fact]
    public async Task Update_ExistingCondominium_ReturnsUpdatedDto()
    {
        // Arrange
        var dto = TestDataBuilder.Condominium();
        var (_, created) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);
        _createdIds.Add(created!.Id);

        // Costruisce l'UpdateDto a partire dal ReadDto esistente (mantiene tutti i campi)
        // e sovrascrive solo il nome
        var updateDto = TestDataBuilder.UpdateCondominium(created, name: $"Aggiornato {TestDataBuilder.ShortId()}");

        // Act
        var (response, updated) = await PutAsync<CondominiumReadDto>(
            $"/api/condominiums/{created.Id}", updateDto);

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Name.Should().Be(updateDto.Name);
    }

    /// <summary>
    /// Verifica che la PUT su un id inesistente (999999) restituisca 404 Not Found.
    /// Il consumer lancia <c>NotFoundException</c> che il filtro globale traduce in 404.
    /// </summary>
    [Fact]
    public async Task Update_NonExistingId_Returns404()
    {
        var updateDto = TestDataBuilder.UpdateCondominium(
            new CondominiumReadDto
            {
                Name                 = "Ghost",
                Code                 = "GHOST",
                InstallmentFrequency = "Monthly",
                Address              = new CondominiumAddressDto()
            });
        var (response, _) = await PutAsync<CondominiumReadDto>("/api/condominiums/999999", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/condominiums/{id} ──────────────────────────────────────

    /// <summary>
    /// Verifica che la DELETE su un condominio esistente restituisca 204 NoContent
    /// e che una GET successiva sullo stesso Id restituisca 404 Not Found
    /// (soft-delete verificato end-to-end).
    /// </summary>
    [Fact]
    public async Task Delete_ExistingCondominium_Returns204()
    {
        // Arrange — crea un condominio da eliminare (non aggiunto a _createdIds perché sarà cancellato)
        var dto = TestDataBuilder.Condominium();
        var (_, created) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);

        // Act
        var response = await DeleteAsync($"/api/condominiums/{created!.Id}");

        // Assert: risposta di cancellazione
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verifica che il condominio non sia più accessibile dopo il soft-delete
        var getResponse = await Client.GetAsync($"/api/condominiums/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifica che la DELETE su un id inesistente (999999) restituisca 404 Not Found.
    /// </summary>
    [Fact]
    public async Task Delete_NonExistingId_Returns404()
    {
        var response = await DeleteAsync("/api/condominiums/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Lifecycle ──────────────────────────────────────────────────────────

    /// <summary>Nessun prerequisito richiesto: ogni test crea i propri dati.</summary>
    public override Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Pulizia best-effort: tenta di eliminare tutti i condomìni creati durante il test.
    /// Gli errori di cleanup vengono ignorati (il DB potrebbe essere già ripristinato
    /// o il condominio già eliminato dal test stesso).
    /// </summary>
    public override async Task DisposeAsync()
    {
        foreach (var id in _createdIds)
        {
            try { await DeleteAsync($"/api/condominiums/{id}"); }
            catch { /* ignora errori di cleanup */ }
        }
        await base.DisposeAsync();
    }
}
