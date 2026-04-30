using System.Net;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Supplier;
using DomuWave.Services.Dto.SupplierContract;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.Supplier;

/// <summary>
/// Test di integrazione per i contratti fornitore (api/supplier-contracts).
///
/// PREREQUISITI: ogni test dispone di un condominio e di un fornitore creati in InitializeAsync.
///
/// VALIDAZIONI COPERTE:
/// <list type="bullet">
///   <item>Creazione con dati validi → 201 Created</item>
///   <item>Creazione senza oggetto → 400 Bad Request</item>
///   <item>Creazione con fornitore inesistente → 404 Not Found</item>
///   <item>Creazione con condominio inesistente → 404 Not Found</item>
///   <item>GET per condominio → include contratto creato</item>
///   <item>GET per fornitore → include contratto creato</item>
///   <item>GET contratti attivi → filtra per stato Active</item>
///   <item>Aggiornamento → 200 OK con dati aggiornati</item>
///   <item>Eliminazione → 204 No Content</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class SupplierContractTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _supplierId;
    private readonly List<int> _contractIds = [];

    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums", TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;

        var (_, supplier) = await PostAsync<SupplierReadDto>(
            "/api/suppliers", TestDataBuilder.Supplier());
        _supplierId = supplier!.Id;
    }

    public override async Task DisposeAsync()
    {
        foreach (var id in _contractIds)
            try { await DeleteAsync($"/api/supplier-contracts/{id}"); } catch { }

        try { await DeleteAsync($"/api/suppliers/{_supplierId}"); } catch { }
        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    // ── POST /api/supplier-contracts ──────────────────────────────────────

    [Fact]
    public async Task Create_ValidContract_Returns201WithSubject()
    {
        var dto = TestDataBuilder.SupplierContract(_condominiumId, _supplierId);

        var (response, created) = await PostAsync<SupplierContractReadDto>("/api/supplier-contracts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Subject.Should().Be(dto.Subject);
        created.CondominiumId.Should().Be(_condominiumId);
        created.SupplierId.Should().Be(_supplierId);

        _contractIds.Add(created.Id);
    }

    [Fact]
    public async Task Create_EmptySubject_Returns400()
    {
        var dto = TestDataBuilder.SupplierContract(_condominiumId, _supplierId, subject: "");

        var (response, _) = await PostAsync<SupplierContractReadDto>("/api/supplier-contracts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await ReadErrorAsync(response);
        error.Should().Contain("oggetto");
    }

    [Fact]
    public async Task Create_NonExistingSupplier_Returns404()
    {
        var dto = TestDataBuilder.SupplierContract(_condominiumId, supplierId: 999999);

        var (response, _) = await PostAsync<SupplierContractReadDto>("/api/supplier-contracts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_NonExistingCondominium_Returns404()
    {
        var dto = TestDataBuilder.SupplierContract(condominiumId: 999999, _supplierId);

        var (response, _) = await PostAsync<SupplierContractReadDto>("/api/supplier-contracts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/supplier-contracts/condominium/{id} ───────────────────────

    [Fact]
    public async Task GetByCondominium_IncludesCreatedContract()
    {
        var (_, created) = await PostAsync<SupplierContractReadDto>(
            "/api/supplier-contracts",
            TestDataBuilder.SupplierContract(_condominiumId, _supplierId));
        _contractIds.Add(created!.Id);

        var list = await GetAsync<IList<SupplierContractReadDto>>(
            $"/api/supplier-contracts/condominium/{_condominiumId}");

        list.Should().Contain(c => c.Id == created.Id);
    }

    // ── GET /api/supplier-contracts/supplier/{id} ──────────────────────────

    [Fact]
    public async Task GetBySupplier_IncludesCreatedContract()
    {
        var (_, created) = await PostAsync<SupplierContractReadDto>(
            "/api/supplier-contracts",
            TestDataBuilder.SupplierContract(_condominiumId, _supplierId));
        _contractIds.Add(created!.Id);

        var list = await GetAsync<IList<SupplierContractReadDto>>(
            $"/api/supplier-contracts/supplier/{_supplierId}");

        list.Should().Contain(c => c.Id == created.Id);
    }

    // ── GET /api/supplier-contracts/condominium/{id}/active ───────────────

    [Fact]
    public async Task GetActive_ReturnsContractsWithActiveStatus()
    {
        var (_, created) = await PostAsync<SupplierContractReadDto>(
            "/api/supplier-contracts",
            TestDataBuilder.SupplierContract(_condominiumId, _supplierId));
        _contractIds.Add(created!.Id);

        var active = await GetAsync<IList<SupplierContractReadDto>>(
            $"/api/supplier-contracts/condominium/{_condominiumId}/active");

        active.Should().Contain(c => c.Id == created.Id);
        active.Should().OnlyContain(c => c.Status == "Active");
    }

    // ── GET /api/supplier-contracts/{id} ──────────────────────────────────

    [Fact]
    public async Task GetById_ExistingContract_ReturnsContract()
    {
        var (_, created) = await PostAsync<SupplierContractReadDto>(
            "/api/supplier-contracts",
            TestDataBuilder.SupplierContract(_condominiumId, _supplierId));
        _contractIds.Add(created!.Id);

        var result = await GetAsync<SupplierContractReadDto>($"/api/supplier-contracts/{created.Id}");

        result.Id.Should().Be(created.Id);
        result.SupplierId.Should().Be(_supplierId);
    }

    [Fact]
    public async Task GetById_NonExistingContract_Returns404()
    {
        var response = await Client.GetAsync("/api/supplier-contracts/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/supplier-contracts/{id} ──────────────────────────────────

    [Fact]
    public async Task Update_ExistingContract_Returns200WithUpdatedSubject()
    {
        var (_, created) = await PostAsync<SupplierContractReadDto>(
            "/api/supplier-contracts",
            TestDataBuilder.SupplierContract(_condominiumId, _supplierId));
        _contractIds.Add(created!.Id);

        var newSubject = $"Contratto aggiornato {TestDataBuilder.ShortId()}";
        var updateDto = new UpdateSupplierContractDto
        {
            Subject       = newSubject,
            StartDate     = created.StartDate,
            AutoRenewal   = false,
            Status        = "Active",
        };

        var (response, updated) = await PutAsync<SupplierContractReadDto>(
            $"/api/supplier-contracts/{created.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Subject.Should().Be(newSubject);
    }

    // ── DELETE /api/supplier-contracts/{id} ───────────────────────────────

    [Fact]
    public async Task Delete_ExistingContract_Returns204()
    {
        var (_, created) = await PostAsync<SupplierContractReadDto>(
            "/api/supplier-contracts",
            TestDataBuilder.SupplierContract(_condominiumId, _supplierId));

        var response = await DeleteAsync($"/api/supplier-contracts/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
