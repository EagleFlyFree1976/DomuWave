using System.Net;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.UnitTenant;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.UnitTenant;

/// <summary>
/// Test di integrazione per gli inquilini di unità immobiliari (api/unit-tenants).
///
/// PREREQUISITI: condominio + unità immobiliare creati in InitializeAsync.
///
/// VALIDAZIONI COPERTE:
/// <list type="bullet">
///   <item>Creazione con dati validi → 201 Created</item>
///   <item>Creazione con unità inesistente → 404 Not Found</item>
///   <item>GET per unità → include l'inquilino creato</item>
///   <item>GET per id → 200 OK</item>
///   <item>GET per id inesistente → 404</item>
///   <item>Aggiornamento → 200 OK con dati aggiornati</item>
///   <item>Eliminazione → 204 No Content</item>
///   <item>Più inquilini sulla stessa unità (storico) → tutti presenti in GetByUnit</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class UnitTenantTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _unitId;
    private readonly List<int> _tenantIds = [];

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
        foreach (var id in _tenantIds)
            try { await DeleteAsync($"/api/unit-tenants/{id}"); } catch { }

        try { await DeleteAsync($"/api/real-estate-units/{_unitId}"); } catch { }
        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    private async Task<UnitTenantReadDto> CreateTenantAsync(
        string? firstName = null, string? lastName = null)
    {
        var dto = TestDataBuilder.UnitTenant(_unitId, firstName, lastName);
        var (_, tenant) = await PostAsync<UnitTenantReadDto>("/api/unit-tenants", dto);
        _tenantIds.Add(tenant!.Id);
        return tenant;
    }

    // ── POST /api/unit-tenants ────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidTenant_Returns201WithUnitId()
    {
        var dto = TestDataBuilder.UnitTenant(_unitId);

        var (response, created) = await PostAsync<UnitTenantReadDto>("/api/unit-tenants", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.UnitId.Should().Be(_unitId);
        created.FirstName.Should().Be(dto.FirstName);
        created.LastName.Should().Be(dto.LastName);
        created.IsActive.Should().BeTrue();
        created.LeaseStartDate.Should().Be(dto.LeaseStartDate);

        _tenantIds.Add(created.Id);
    }

    [Fact]
    public async Task Create_NonExistingUnit_Returns404()
    {
        var dto = TestDataBuilder.UnitTenant(unitId: 999999);

        var (response, _) = await PostAsync<UnitTenantReadDto>("/api/unit-tenants", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/unit-tenants/by-unit/{unitId} ────────────────────────────

    [Fact]
    public async Task GetByUnit_IncludesCreatedTenant()
    {
        var tenant = await CreateTenantAsync();

        var list = await GetAsync<IList<UnitTenantReadDto>>(
            $"/api/unit-tenants/by-unit/{_unitId}");

        list.Should().Contain(t => t.Id == tenant.Id);
    }

    [Fact]
    public async Task GetByUnit_MultipleTenants_ReturnsAll()
    {
        var tenant1 = await CreateTenantAsync(firstName: "Luigi",   lastName: "Verdi");
        var tenant2 = await CreateTenantAsync(firstName: "Paola",   lastName: "Neri");

        var list = await GetAsync<IList<UnitTenantReadDto>>(
            $"/api/unit-tenants/by-unit/{_unitId}");

        list.Select(t => t.Id).Should().Contain([tenant1.Id, tenant2.Id]);
    }

    // ── GET /api/unit-tenants/{id} ────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingTenant_ReturnsTenant()
    {
        var tenant = await CreateTenantAsync();

        var result = await GetAsync<UnitTenantReadDto>($"/api/unit-tenants/{tenant.Id}");

        result.Id.Should().Be(tenant.Id);
        result.UnitId.Should().Be(_unitId);
    }

    [Fact]
    public async Task GetById_NonExistingTenant_Returns404()
    {
        var response = await Client.GetAsync("/api/unit-tenants/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/unit-tenants/{id} ────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingTenant_Returns200WithUpdatedEmail()
    {
        var tenant = await CreateTenantAsync();

        var newEmail  = $"aggiornato-{TestDataBuilder.ShortId()}@test.it";
        var updateDto = new UpdateUnitTenantDto
        {
            FirstName      = tenant.FirstName,
            LastName       = tenant.LastName,
            Email          = newEmail,
            Phone          = tenant.Phone,
            LeaseStartDate = tenant.LeaseStartDate,
            LeaseEndDate   = tenant.LeaseEndDate,
            IsActive       = tenant.IsActive,
            ExpensePayer   = tenant.ExpensePayer,
        };

        var (response, updated) = await PutAsync<UnitTenantReadDto>(
            $"/api/unit-tenants/{tenant.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Email.Should().Be(newEmail);
    }

    // ── DELETE /api/unit-tenants/{id} ─────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingTenant_Returns204()
    {
        var tenant = await CreateTenantAsync();
        _tenantIds.Remove(tenant.Id);

        var response = await DeleteAsync($"/api/unit-tenants/{tenant.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistingTenant_Returns404()
    {
        var response = await DeleteAsync("/api/unit-tenants/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Search ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_ByLastName_ReturnsMatchingTenants()
    {
        var uniqueLastName = $"Cerca-{TestDataBuilder.ShortId()}";
        var tenant = await CreateTenantAsync(lastName: uniqueLastName);

        var list = await GetAsync<IList<UnitTenantReadDto>>(
            $"/api/unit-tenants/search?q={uniqueLastName}");

        list.Should().Contain(t => t.Id == tenant.Id);
    }
}
