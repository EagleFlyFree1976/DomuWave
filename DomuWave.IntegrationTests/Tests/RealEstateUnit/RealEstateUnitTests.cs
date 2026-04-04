using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.RealEstateUnit;
using FluentAssertions;
using System.Net;

namespace DomuWave.IntegrationTests.Tests.RealEstateUnit;

/// <summary>
/// Integration tests for /api/real-estate-units.
/// </summary>
[Collection("Integration")]
public class RealEstateUnitTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private readonly List<int> _unitIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────

    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums",
            TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;
    }

    public override async Task DisposeAsync()
    {
        foreach (var id in _unitIds)
        {
            try { await DeleteAsync($"/api/real-estate-units/{id}"); }
            catch { /* ignore */ }
        }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); }
        catch { /* ignore */ }

        await base.DisposeAsync();
    }

    // ── POST /api/real-estate-units ────────────────────────────────────────

    [Fact]
    public async Task Create_ValidUnit_Returns201()
    {
        var dto = TestDataBuilder.RealEstateUnit(_condominiumId);

        var (response, created) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.CondominiumId.Should().Be(_condominiumId);

        _unitIds.Add(created.Id);
    }

    [Fact]
    public async Task Create_MissingCondominiumId_Returns400Or404()
    {
        var dto = TestDataBuilder.RealEstateUnit(condominiumId: 0); // invalid

        var (response, _) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units", dto);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    // ── GET /api/real-estate-units/by-condominium/{id} ────────────────────

    [Fact]
    public async Task GetByCondominium_ReturnsAllUnitsForCondominium()
    {
        // Create 2 units
        var (_, u1) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));
        var (_, u2) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));

        _unitIds.AddRange([u1!.Id, u2!.Id]);

        var list = await GetAsync<IList<RealEstateUnitReadDto>>(
            $"/api/real-estate-units/by-condominium/{_condominiumId}");

        list.Should().Contain(u => u.Id == u1.Id)
            .And.Contain(u => u.Id == u2.Id);
    }

    // ── GET /api/real-estate-units/{id} ───────────────────────────────────

    [Fact]
    public async Task GetById_Existing_ReturnsDto()
    {
        var (_, created) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));
        _unitIds.Add(created!.Id);

        var result = await GetAsync<RealEstateUnitReadDto>($"/api/real-estate-units/{created.Id}");

        result.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await Client.GetAsync("/api/real-estate-units/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/real-estate-units/{id} ───────────────────────────────────

    [Fact]
    public async Task Update_ExistingUnit_ReturnsUpdated()
    {
        var (_, created) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));
        _unitIds.Add(created!.Id);

        var updateDto = new UpdateRealEstateUnitDto
        {
            Notes     = "Nota aggiornata",
            AreaSqm   = 90m,
            IsActive  = true,
        };

        var (response, updated) = await PutAsync<RealEstateUnitReadDto>(
            $"/api/real-estate-units/{created.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.AreaSqm.Should().Be(updateDto.AreaSqm);
    }

    // ── DELETE /api/real-estate-units/{id} ────────────────────────────────

    [Fact]
    public async Task Delete_ExistingUnit_Returns204()
    {
        var (_, created) = await PostAsync<RealEstateUnitReadDto>("/api/real-estate-units",
            TestDataBuilder.RealEstateUnit(_condominiumId));

        var response = await DeleteAsync($"/api/real-estate-units/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
