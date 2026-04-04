using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using FluentAssertions;
using System.Net;

namespace DomuWave.IntegrationTests.Tests.Condominiums;

/// <summary>
/// Integration tests for GET/POST/PUT/DELETE /api/condominiums.
///
/// Each test creates its own data and cleans up on completion,
/// so tests can run in any order against a shared database.
/// </summary>
[Collection("Integration")]
public class CondominiumCrudTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    // Tracks condominiums created during this test run for cleanup
    private readonly List<int> _createdIds = [];

    // ── POST /api/condominiums ─────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidPayload_Returns201WithDto()
    {
        // Arrange
        var dto = TestDataBuilder.Condominium();

        // Act
        var (response, created) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Code.Should().Be(dto.Code);
        created.Name.Should().Be(dto.Name);

        _createdIds.Add(created.Id);
    }

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

    [Fact]
    public async Task Create_DuplicateCode_Returns400()
    {
        // Arrange — create a condominium first
        var dto = TestDataBuilder.Condominium();
        var (_, first) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);
        first.Should().NotBeNull();
        _createdIds.Add(first!.Id);

        // Act — try to create another with the same code
        var (response, _) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await ReadErrorAsync(response);
        error.Should().Contain(dto.Code);
    }

    // ── GET /api/condominiums ──────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var result = await GetAsync<IList<CondominiumReadDto>>("/api/condominiums");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsCondominium()
    {
        // Arrange — create one
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

    [Fact]
    public async Task GetById_NonExistingId_Returns404()
    {
        var response = await Client.GetAsync("/api/condominiums/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

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

    [Fact]
    public async Task Update_ExistingCondominium_ReturnsUpdatedDto()
    {
        // Arrange
        var dto = TestDataBuilder.Condominium();
        var (_, created) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);
        _createdIds.Add(created!.Id);

        var updateDto = new UpdateCondominiumDto
        {
            Name  = $"Aggiornato {TestDataBuilder.ShortId()}",
            Notes = "Nota aggiornata",
        };

        // Act
        var (response, updated) = await PutAsync<CondominiumReadDto>(
            $"/api/condominiums/{created.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task Update_NonExistingId_Returns404()
    {
        var updateDto = new UpdateCondominiumDto { Name = "Ghost" };
        var (response, _) = await PutAsync<CondominiumReadDto>("/api/condominiums/999999", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/condominiums/{id} ──────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingCondominium_Returns204()
    {
        // Arrange
        var dto = TestDataBuilder.Condominium();
        var (_, created) = await PostAsync<CondominiumReadDto>("/api/condominiums", dto);

        // Act
        var response = await DeleteAsync($"/api/condominiums/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's gone
        var getResponse = await Client.GetAsync($"/api/condominiums/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistingId_Returns404()
    {
        var response = await DeleteAsync("/api/condominiums/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Lifecycle ──────────────────────────────────────────────────────────

    public override Task InitializeAsync() => Task.CompletedTask;

    /// <summary>Best-effort cleanup: delete condominiums created during this test.</summary>
    public override async Task DisposeAsync()
    {
        foreach (var id in _createdIds)
        {
            try { await DeleteAsync($"/api/condominiums/{id}"); }
            catch { /* ignore cleanup failures */ }
        }
        await base.DisposeAsync();
    }
}
