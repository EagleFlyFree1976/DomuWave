using System.Net.Http.Json;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Dto.Contabilita.FiscalYear;
using DomuWave.Services.Models;
using FluentAssertions;
using System.Net;
using ChartOfAccountsReadDto = DomuWave.Services.Dto.Budget.ChartOfAccountsReadDto;

namespace DomuWave.IntegrationTests.Tests.Budget;

/// <summary>
/// Integration tests for the Budget CQRS pipeline and the
/// Draft → Approve → Close workflow.
///
/// Each test class run creates its own Condominium + FiscalYear + ChartOfAccounts
/// and tears them down afterwards.
///
/// Endpoint return codes (from BudgetsController):
///   POST /api/budgets               → 201 Created
///   GET  /api/budgets/{id}          → 200 OK | 404 Not Found
///   POST /api/budgets/{id}/approve  → 204 No Content | 400 Bad Request | 404 Not Found
///   POST /api/budgets/{id}/close    → 204 No Content | 400 Bad Request
///   POST /api/budgets/{id}/reopen   → 204 No Content | 400 Bad Request
///   DELETE /api/budgets/{id}        → 204 No Content | 400 Bad Request
/// </summary>
[Collection("Integration")]
public class BudgetWorkflowTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private int _fiscalYearId;
    private readonly List<int> _budgetIds  = [];
    private readonly List<int> _accountIds = [];

    // ── Lifecycle ──────────────────────────────────────────────────────────

    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums",
            TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;

        var (_, fy) = await PostAsync<FiscalYearReadDto>(
            "/api/fiscal-years",
            TestDataBuilder.FiscalYear(_condominiumId));
        _fiscalYearId = fy!.Id;

        await SeedChartOfAccountsAsync();
    }

    public override async Task DisposeAsync()
    {
        foreach (var id in _budgetIds)
        {
            try { await DeleteAsync($"/api/budgets/{id}"); }
            catch { /* ignore */ }
        }

        foreach (var id in _accountIds)
        {
            try { await DeleteAsync($"/api/chart-of-accounts/{id}"); }
            catch { /* ignore */ }
        }

        try { await DeleteAsync($"/api/fiscal-years/{_fiscalYearId}"); }
        catch { /* ignore */ }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); }
        catch { /* ignore */ }

        await base.DisposeAsync();
    }

    // ── POST /api/budgets ──────────────────────────────────────────────────

    [Fact]
    public async Task Create_PreventivoBudget_Returns201InDraftStatus()
    {
        var dto = TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo);

        var (response, created) = await PostAsync<BudgetReadDto>("/api/budgets", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.StatusId.Should().Be(BudgetStatus.Draft, "new budgets must start in Draft");
        created.Type.Should().Be(BudgetType.Preventivo);

        _budgetIds.Add(created.Id);
    }

    [Fact]
    public async Task Create_ConsuntivoBudget_Returns201InDraftStatus()
    {
        var dto = TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Consuntivo);

        var (response, created) = await PostAsync<BudgetReadDto>("/api/budgets", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created!.StatusId.Should().Be(BudgetStatus.Draft);
        created.Type.Should().Be(BudgetType.Consuntivo);

        _budgetIds.Add(created.Id);
    }

    // ── GET /api/budgets/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingBudget_ReturnsBudget()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        var result = await GetAsync<BudgetReadDto>($"/api/budgets/{created.Id}");

        result.Id.Should().Be(created.Id);
        result.CondominiumId.Should().Be(_condominiumId);
        result.FiscalYearId.Should().Be(_fiscalYearId);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await Client.GetAsync("/api/budgets/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByCondominium_IncludesCreatedBudget()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        var list = await GetAsync<IList<BudgetReadDto>>(
            $"/api/budgets/by-condominium/{_condominiumId}");

        list.Should().Contain(b => b.Id == created.Id);
    }

    // ── Workflow: Draft → Approve ──────────────────────────────────────────

    [Fact]
    public async Task Approve_BudgetInDraft_Returns204AndStatusBecomesApproved()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        var approveResponse = await ApproveBudgetAsync(created.Id);

        approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "successful approve returns 204 NoContent");

        var budget = await GetAsync<BudgetReadDto>($"/api/budgets/{created.Id}");
        budget.StatusId.Should().Be(BudgetStatus.Approved);
    }

    [Fact]
    public async Task Approve_AlreadyApprovedBudget_Returns400()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        await ApproveBudgetAsync(created.Id); // first approval

        var secondApprove = await ApproveBudgetAsync(created.Id); // second attempt
        secondApprove.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Approve_WhenConflictingApprovedBudgetExists_Returns400()
    {
        // Create and approve the first budget
        var (_, first) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo));
        _budgetIds.Add(first!.Id);
        await ApproveBudgetAsync(first.Id);

        // Create a second Preventivo in the same condominium+year
        var (_, second) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId, BudgetType.Preventivo));
        _budgetIds.Add(second!.Id);

        // Approval should fail because a conflicting approved budget already exists
        var response = await ApproveBudgetAsync(second.Id);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Workflow: Approve → Close ──────────────────────────────────────────

    [Fact]
    public async Task Close_ApprovedBudget_Returns204AndStatusBecomesClosed()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        await ApproveBudgetAsync(created.Id);

        var closeResponse = await Client.PostAsJsonAsync(
            $"/api/budgets/{created.Id}/close", new { });

        closeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var budget = await GetAsync<BudgetReadDto>($"/api/budgets/{created.Id}");
        budget.StatusId.Should().Be(BudgetStatus.Closed);
    }

    [Fact]
    public async Task Close_DraftBudget_Returns400()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        // Skip approval — close directly from Draft
        var response = await Client.PostAsJsonAsync(
            $"/api/budgets/{created.Id}/close", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Workflow: Approve → Reopen ─────────────────────────────────────────

    [Fact]
    public async Task Reopen_ApprovedBudget_Returns204AndStatusReturnsToDraft()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        await ApproveBudgetAsync(created.Id);

        var reopenResponse = await Client.PostAsJsonAsync(
            $"/api/budgets/{created.Id}/reopen", new { });

        reopenResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var budget = await GetAsync<BudgetReadDto>($"/api/budgets/{created.Id}");
        budget.StatusId.Should().Be(BudgetStatus.Draft);
    }

    // ── DELETE /api/budgets/{id} ───────────────────────────────────────────

    [Fact]
    public async Task Delete_DraftBudget_Returns204()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));

        var response = await DeleteAsync($"/api/budgets/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ApprovedBudget_Returns400()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id); // keep for cleanup since delete will fail

        await ApproveBudgetAsync(created.Id);

        var response = await DeleteAsync($"/api/budgets/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "approved budgets cannot be deleted");
    }

    // ── PUT /api/budgets/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task Update_DraftBudget_Returns200()
    {
        var (_, created) = await PostAsync<BudgetReadDto>("/api/budgets",
            TestDataBuilder.Budget(_condominiumId, _fiscalYearId));
        _budgetIds.Add(created!.Id);

        var updateDto = new UpdateBudgetDto { Notes = "Note aggiornate" };

        var (response, updated) = await PutAsync<BudgetReadDto>(
            $"/api/budgets/{created.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Notes.Should().Be(updateDto.Notes);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private Task<HttpResponseMessage> ApproveBudgetAsync(int budgetId)
        => Client.PostAsJsonAsync($"/api/budgets/{budgetId}/approve", new
        {
            NumberOfInstallments = 4,
            FirstDueDate         = DateTime.Today,
        });

    private async Task SeedChartOfAccountsAsync()
    {
        // ApproveBudgetCommandConsumer requires at least one account per type.
        var types = new[]
        {
            (Type: ChartOfAccountsType.Entrata,      Name: "Entrate Test"),
            (Type: ChartOfAccountsType.Uscita,       Name: "Uscite Test"),
            (Type: ChartOfAccountsType.Patrimoniale, Name: "Patrimonio Test"),
        };

        foreach (var (type, name) in types)
        {
            var (_, acc) = await PostAsync<ChartOfAccountsReadDto>(
                "/api/chart-of-accounts",
                new CreateChartOfAccountsDto
                {
                    CondominiumId = _condominiumId,
                    Code          = $"ACC-{TestDataBuilder.ShortId()}",
                    Name          = name,
                    Type          = type,
                    IsActive      = true,
                });

            if (acc != null)
                _accountIds.Add(acc.Id);
        }
    }
}
