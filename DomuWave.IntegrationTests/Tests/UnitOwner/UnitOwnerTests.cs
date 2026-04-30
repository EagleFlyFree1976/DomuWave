using System.Net;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.RealEstateUnit;
using DomuWave.Services.Dto.UnitOwner;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.UnitOwner;

/// <summary>
/// Test di integrazione per i proprietari di unità immobiliari (api/unit-owners).
///
/// PREREQUISITI: condominio + unità immobiliare creati in InitializeAsync.
///
/// VALIDAZIONI COPERTE:
/// <list type="bullet">
///   <item>Creazione con dati validi → 201 Created</item>
///   <item>Creazione con unità inesistente → 404 Not Found</item>
///   <item>GET per unità → include il proprietario creato</item>
///   <item>GET per id → 200 OK</item>
///   <item>GET per id inesistente → 404</item>
///   <item>Aggiornamento → 200 OK con dati aggiornati</item>
///   <item>Eliminazione → 204 No Content</item>
///   <item>Più proprietari sulla stessa unità → tutti presenti in GetByUnit</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class UnitOwnerTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _unitId;
    private readonly List<int> _ownerIds = [];

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
        foreach (var id in _ownerIds)
            try { await DeleteAsync($"/api/unit-owners/{id}"); } catch { }

        try { await DeleteAsync($"/api/real-estate-units/{_unitId}"); } catch { }
        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    private async Task<UnitOwnerReadDto> CreateOwnerAsync(
        string? firstName = null, string? lastName = null)
    {
        var dto = TestDataBuilder.UnitOwner(_unitId, firstName, lastName);
        var (_, owner) = await PostAsync<UnitOwnerReadDto>("/api/unit-owners", dto);
        _ownerIds.Add(owner!.Id);
        return owner;
    }

    // ── POST /api/unit-owners ─────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidOwner_Returns201WithUnitId()
    {
        var dto = TestDataBuilder.UnitOwner(_unitId);

        var (response, created) = await PostAsync<UnitOwnerReadDto>("/api/unit-owners", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.UnitId.Should().Be(_unitId);
        created.FirstName.Should().Be(dto.FirstName);
        created.LastName.Should().Be(dto.LastName);
        created.IsActive.Should().BeTrue();
        created.OwnershipQuota.Should().Be(100m);

        _ownerIds.Add(created.Id);
    }

    [Fact]
    public async Task Create_NonExistingUnit_Returns404()
    {
        var dto = TestDataBuilder.UnitOwner(unitId: 999999);

        var (response, _) = await PostAsync<UnitOwnerReadDto>("/api/unit-owners", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/unit-owners/by-unit/{unitId} ─────────────────────────────

    [Fact]
    public async Task GetByUnit_IncludesCreatedOwner()
    {
        var owner = await CreateOwnerAsync();

        var list = await GetAsync<IList<UnitOwnerReadDto>>(
            $"/api/unit-owners/by-unit/{_unitId}");

        list.Should().Contain(o => o.Id == owner.Id);
    }

    [Fact]
    public async Task GetByUnit_MultipleOwners_ReturnsAll()
    {
        var owner1 = await CreateOwnerAsync(firstName: "Mario",   lastName: "Rossi");
        var owner2 = await CreateOwnerAsync(firstName: "Giovanna", lastName: "Bianchi");

        var list = await GetAsync<IList<UnitOwnerReadDto>>(
            $"/api/unit-owners/by-unit/{_unitId}");

        list.Select(o => o.Id).Should().Contain([owner1.Id, owner2.Id]);
    }

    // ── GET /api/unit-owners/{id} ─────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingOwner_ReturnsOwner()
    {
        var owner = await CreateOwnerAsync();

        var result = await GetAsync<UnitOwnerReadDto>($"/api/unit-owners/{owner.Id}");

        result.Id.Should().Be(owner.Id);
        result.UnitId.Should().Be(_unitId);
    }

    [Fact]
    public async Task GetById_NonExistingOwner_Returns404()
    {
        var response = await Client.GetAsync("/api/unit-owners/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/unit-owners/{id} ─────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingOwner_Returns200WithUpdatedLastName()
    {
        var owner = await CreateOwnerAsync();

        var newLastName = $"Aggiornato {TestDataBuilder.ShortId()}";
        var updateDto = new UpdateUnitOwnerDto
        {
            UserId          = owner.UserId,
            FirstName       = owner.FirstName,
            LastName        = newLastName,
            Email           = owner.Email,
            OwnerType       = owner.OwnerType,
            OwnershipQuota  = owner.OwnershipQuota,
            StartDate       = owner.StartDate,
            IsResident      = owner.IsResident,
            IsActive        = owner.IsActive,
            IsAccessEnabled = owner.IsAccessEnabled,
        };

        var (response, updated) = await PutAsync<UnitOwnerReadDto>(
            $"/api/unit-owners/{owner.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.LastName.Should().Be(newLastName);
    }

    // ── DELETE /api/unit-owners/{id} ──────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingOwner_Returns204()
    {
        var owner = await CreateOwnerAsync();
        _ownerIds.Remove(owner.Id); // gestiamo l'eliminazione nel test

        var response = await DeleteAsync($"/api/unit-owners/{owner.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistingOwner_Returns404()
    {
        var response = await DeleteAsync("/api/unit-owners/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Search ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_ByLastName_ReturnsMatchingOwners()
    {
        var uniqueLastName = $"Cerca-{TestDataBuilder.ShortId()}";
        var owner = await CreateOwnerAsync(lastName: uniqueLastName);

        var list = await GetAsync<IList<UnitOwnerReadDto>>(
            $"/api/unit-owners/search?q={uniqueLastName}");

        list.Should().Contain(o => o.Id == owner.Id);
    }
}
