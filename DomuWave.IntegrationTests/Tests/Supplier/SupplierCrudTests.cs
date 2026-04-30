using System.Net;
using System.Net.Http.Json;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Supplier;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.Supplier;

/// <summary>
/// Test di integrazione per il CRUD dei fornitori (api/suppliers).
///
/// VALIDAZIONI COPERTE:
/// <list type="bullet">
///   <item>Creazione con dati validi → 201 Created con CompanyName corretto</item>
///   <item>Creazione senza CompanyName → 400 Bad Request</item>
///   <item>GET per id esistente → 200 OK</item>
///   <item>GET per id inesistente → 404 Not Found</item>
///   <item>Lista completa → include il fornitore creato</item>
///   <item>Aggiornamento → 200 OK con dati aggiornati</item>
///   <item>Eliminazione → 204 No Content; seconda eliminazione → 404</item>
///   <item>Ricerca per tipo → filtra correttamente</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class SupplierCrudTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private readonly List<int> _supplierIds = [];

    public override Task InitializeAsync() => Task.CompletedTask;

    public override async Task DisposeAsync()
    {
        foreach (var id in _supplierIds)
            try { await DeleteAsync($"/api/suppliers/{id}"); } catch { }

        await base.DisposeAsync();
    }

    // ── POST /api/suppliers ────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidSupplier_Returns201WithCompanyName()
    {
        var dto = TestDataBuilder.Supplier();

        var (response, created) = await PostAsync<SupplierReadDto>("/api/suppliers", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.CompanyName.Should().Be(dto.CompanyName);
        created.IsActive.Should().BeTrue();

        _supplierIds.Add(created.Id);
    }

    [Fact]
    public async Task Create_EmptyCompanyName_Returns400()
    {
        var dto = TestDataBuilder.Supplier(companyName: "");

        var (response, _) = await PostAsync<SupplierReadDto>("/api/suppliers", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "la ragione sociale è obbligatoria");
    }

    [Fact]
    public async Task Create_WhitespaceCompanyName_Returns400()
    {
        var dto = TestDataBuilder.Supplier(companyName: "   ");

        var (response, _) = await PostAsync<SupplierReadDto>("/api/suppliers", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await ReadErrorAsync(response);
        error.Should().Contain("ragione sociale");
    }

    // ── GET /api/suppliers/{id} ────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingSupplier_ReturnsSupplier()
    {
        var (_, created) = await PostAsync<SupplierReadDto>("/api/suppliers", TestDataBuilder.Supplier());
        _supplierIds.Add(created!.Id);

        var result = await GetAsync<SupplierReadDto>($"/api/suppliers/{created.Id}");

        result.Id.Should().Be(created.Id);
        result.CompanyName.Should().Be(created.CompanyName);
    }

    [Fact]
    public async Task GetById_NonExistingSupplier_Returns404()
    {
        var response = await Client.GetAsync("/api/suppliers/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/suppliers ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_IncludesCreatedSupplier()
    {
        var (_, created) = await PostAsync<SupplierReadDto>("/api/suppliers", TestDataBuilder.Supplier());
        _supplierIds.Add(created!.Id);

        var list = await GetAsync<IList<SupplierReadDto>>("/api/suppliers");

        list.Should().Contain(s => s.Id == created.Id);
    }

    // ── GET /api/suppliers/by-type/{type} ──────────────────────────────────

    [Fact]
    public async Task GetByType_ReturnsOnlySuppliersOfThatType()
    {
        var uniqueType = $"Tipo-{TestDataBuilder.ShortId()}";
        var dto = TestDataBuilder.Supplier(supplierType: uniqueType);
        var (_, created) = await PostAsync<SupplierReadDto>("/api/suppliers", dto);
        _supplierIds.Add(created!.Id);

        var list = await GetAsync<IList<SupplierReadDto>>($"/api/suppliers/by-type/{uniqueType}");

        list.Should().Contain(s => s.Id == created.Id);
        list.Should().OnlyContain(s => s.SupplierType == uniqueType);
    }

    // ── PUT /api/suppliers/{id} ────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingSupplier_Returns200WithUpdatedName()
    {
        var (_, created) = await PostAsync<SupplierReadDto>("/api/suppliers", TestDataBuilder.Supplier());
        _supplierIds.Add(created!.Id);

        var updatedName = $"Fornitore Aggiornato {TestDataBuilder.ShortId()}";
        var updateDto   = TestDataBuilder.UpdateSupplier(created, companyName: updatedName);
        var (response, updated) = await PutAsync<SupplierReadDto>($"/api/suppliers/{created.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.CompanyName.Should().Be(updatedName);
    }

    [Fact]
    public async Task Update_NonExistingSupplier_Returns404OrNull()
    {
        var dto = TestDataBuilder.UpdateSupplier(new SupplierReadDto
        {
            CompanyName  = "Test",
            IsActive     = true,
        });

        var response = await Client.PutAsJsonAsync("/api/suppliers/999999", dto);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    // ── DELETE /api/suppliers/{id} ─────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingSupplier_Returns204()
    {
        var (_, created) = await PostAsync<SupplierReadDto>("/api/suppliers", TestDataBuilder.Supplier());

        var response = await DeleteAsync($"/api/suppliers/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_Returns404()
    {
        var (_, created) = await PostAsync<SupplierReadDto>("/api/suppliers", TestDataBuilder.Supplier());
        await DeleteAsync($"/api/suppliers/{created!.Id}");

        var secondDelete = await DeleteAsync($"/api/suppliers/{created.Id}");

        secondDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistingSupplier_Returns404()
    {
        var response = await DeleteAsync("/api/suppliers/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
