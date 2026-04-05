using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using FluentAssertions;
using System.Net;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.FiscalYear;

/// <summary>
/// Integration tests for /api/fiscal-years.
/// </summary>
[Collection("Integration")]
public class FiscalYearTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private readonly List<int> _fiscalYearIds    = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────

    public override async Task InitializeAsync()
    {
        // Create a fresh condominium for each test class run
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums",
            TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;
    }

    public override async Task DisposeAsync()
    {
        foreach (var id in _fiscalYearIds)
        {
            try { await DeleteAsync($"/api/fiscal-years/{id}"); }
            catch { /* ignore */ }
        }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); }
        catch { /* ignore */ }

        await base.DisposeAsync();
    }

    // ── POST /api/fiscal-years ─────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidFiscalYear_Returns201()
    {
        var dto = TestDataBuilder.FiscalYear(_condominiumId);

        var (response, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Code.Should().Be(dto.Code);
        created.CondominiumId.Should().Be(_condominiumId);

        _fiscalYearIds.Add(created.Id);
    }

    [Fact]
    public async Task Create_EndDateBeforeStartDate_Returns400()
    {
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        dto.EndDate = dto.StartDate.AddDays(-1); // invalid

        var (response, _) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_DuplicateCode_Returns400()
    {
        var dto = TestDataBuilder.FiscalYear(_condominiumId);

        var (_, first) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);
        _fiscalYearIds.Add(first!.Id);

        // second attempt with same code
        var (response, _) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/fiscal-years/by-condominium/{id} ──────────────────────────

    [Fact]
    public async Task GetByCondominium_ReturnsListIncludingCreated()
    {
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        var (_, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);
        _fiscalYearIds.Add(created!.Id);

        var list = await GetAsync<IList<FiscalYearReadDto>>(
            $"/api/fiscal-years/by-condominium/{_condominiumId}");

        list.Should().Contain(fy => fy.Id == created.Id);
    }

    // ── GET /api/fiscal-years/{id} ─────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingFiscalYear_ReturnsDto()
    {
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        var (_, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);
        _fiscalYearIds.Add(created!.Id);

        var result = await GetAsync<FiscalYearReadDto>($"/api/fiscal-years/{created.Id}");

        result.Id.Should().Be(created.Id);
        result.CondominiumId.Should().Be(_condominiumId);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await Client.GetAsync("/api/fiscal-years/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/fiscal-years/{id} ──────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingFiscalYear_Returns204()
    {
        var dto = TestDataBuilder.FiscalYear(_condominiumId);
        var (_, created) = await PostAsync<FiscalYearReadDto>("/api/fiscal-years", dto);

        var response = await DeleteAsync($"/api/fiscal-years/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
