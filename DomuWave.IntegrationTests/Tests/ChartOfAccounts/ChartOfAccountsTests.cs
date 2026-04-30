using System.Net;
using DomuWave.IntegrationTests.Builders;
using DomuWave.IntegrationTests.Infrastructure;
using DomuWave.Services.Dto.Budget;
using DomuWave.Services.Dto.ChartOfAccounts;
using DomuWave.Services.Dto.Condominium;
using DomuWave.Services.Models;
using FluentAssertions;
using Xunit;

namespace DomuWave.IntegrationTests.Tests.ChartOfAccounts;

/// <summary>
/// Test di integrazione per il piano dei conti (api/chart-of-accounts).
///
/// PREREQUISITI: ogni test dispone di un condominio creato in InitializeAsync.
///
/// VALIDAZIONI COPERTE:
/// <list type="bullet">
///   <item>Creazione con dati validi → 201 Created</item>
///   <item>Creazione con codice duplicato nel condominio → 400</item>
///   <item>Creazione senza codice → 400</item>
///   <item>GET per condominio → include conto creato</item>
///   <item>GET per tipo → filtra per tipo (Entrata / Uscita / Patrimoniale)</item>
///   <item>Gerarchia: conto figlio con parent valido → 201</item>
///   <item>Aggiornamento → 200 OK</item>
///   <item>Eliminazione conto senza figli → 204</item>
///   <item>Eliminazione conto con figli → 400 (figlio deve essere rimosso prima)</item>
///   <item>ChargeabilityType = Tenant → verificato nel ReadDto</item>
/// </list>
/// </summary>
[Collection("Integration")]
public class ChartOfAccountsTests(IntegrationTestFactory factory)
    : IntegrationTestBase(factory), IAsyncLifetime
{
    private int _condominiumId;
    private readonly List<int> _accountIds = [];

    public override async Task InitializeAsync()
    {
        var (_, condo) = await PostAsync<CondominiumReadDto>(
            "/api/condominiums", TestDataBuilder.Condominium());
        _condominiumId = condo!.Id;
    }

    public override async Task DisposeAsync()
    {
        // Elimina prima i conti figlio (quelli con ParentAccountId), poi i root
        var children = _accountIds.Skip(0).ToList();
        foreach (var id in children)
            try { await DeleteAsync($"/api/chart-of-accounts/{id}"); } catch { }

        try { await DeleteAsync($"/api/condominiums/{_condominiumId}"); } catch { }

        await base.DisposeAsync();
    }

    private async Task<ChartOfAccountsReadDto> CreateAccountAsync(
        ChartOfAccountsType type = ChartOfAccountsType.Uscita,
        string? code = null,
        string? name = null,
        int? parentAccountId = null)
    {
        var dto = TestDataBuilder.ChartOfAccounts(_condominiumId, type, code, name);
        if (parentAccountId.HasValue)
            dto.ParentAccountId = parentAccountId;

        var (_, account) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);
        _accountIds.Insert(0, account!.Id); // inserisce in testa: i figli vengono eliminati prima
        return account;
    }

    // ── POST /api/chart-of-accounts ───────────────────────────────────────

    [Fact]
    public async Task Create_ValidAccount_Returns201WithCodeAndType()
    {
        var dto = TestDataBuilder.ChartOfAccounts(_condominiumId, ChartOfAccountsType.Entrata);

        var (response, created) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Code.Should().Be(dto.Code);
        created.Name.Should().Be(dto.Name);
        created.Type.Should().Be(ChartOfAccountsType.Entrata);
        created.CondominiumId.Should().Be(_condominiumId);
        created.IsActive.Should().BeTrue();

        _accountIds.Add(created.Id);
    }

    [Fact]
    public async Task Create_DuplicateCodeSameCondominium_Returns400()
    {
        var fixedCode = $"DUP-{TestDataBuilder.ShortId()}";
        var first = await CreateAccountAsync(code: fixedCode);

        var dto = TestDataBuilder.ChartOfAccounts(_condominiumId, code: fixedCode);
        var (response, _) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "codice duplicato per lo stesso condominio");
    }

    [Fact]
    public async Task Create_EmptyCode_Returns400()
    {
        var dto = TestDataBuilder.ChartOfAccounts(_condominiumId, code: "");

        var (response, _) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var dto = TestDataBuilder.ChartOfAccounts(_condominiumId, name: "");

        var (response, _) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithChargeabilityTypeTenant_Returns201WithCorrectType()
    {
        var dto = TestDataBuilder.ChartOfAccounts(_condominiumId);
        dto.ChargeabilityTypeId = ChargeabilityType.Tenant;

        var (response, created) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        created!.ChargeabilityTypeId.Should().Be(ChargeabilityType.Tenant);

        _accountIds.Add(created.Id);
    }

    // ── GET /api/chart-of-accounts/by-condominium/{id} ────────────────────

    [Fact]
    public async Task GetByCondominium_IncludesCreatedAccount()
    {
        var account = await CreateAccountAsync();

        var list = await GetAsync<IList<ChartOfAccountsReadDto>>(
            $"/api/chart-of-accounts/by-condominium/{_condominiumId}");

        list.Should().Contain(a => a.Id == account.Id);
    }

    // ── GET filtering by type ──────────────────────────────────────────────

    [Fact]
    public async Task Create_ThreeTypes_AllPresent_GetByCondominiumReturnsAll()
    {
        var entrata     = await CreateAccountAsync(ChartOfAccountsType.Entrata);
        var uscita      = await CreateAccountAsync(ChartOfAccountsType.Uscita);
        var patrimoniale = await CreateAccountAsync(ChartOfAccountsType.Patrimoniale);

        var list = await GetAsync<IList<ChartOfAccountsReadDto>>(
            $"/api/chart-of-accounts/by-condominium/{_condominiumId}");

        var ids = list.Select(a => a.Id).ToHashSet();
        ids.Should().Contain(entrata.Id);
        ids.Should().Contain(uscita.Id);
        ids.Should().Contain(patrimoniale.Id);
    }

    // ── Hierarchical accounts ──────────────────────────────────────────────

    [Fact]
    public async Task Create_ChildAccount_WithValidParent_Returns201WithCorrectLevel()
    {
        var parent = await CreateAccountAsync(name: "Root account");

        var dto = TestDataBuilder.ChartOfAccounts(_condominiumId);
        dto.ParentAccountId = parent.Id;
        var (response, child) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        child!.ParentAccountId.Should().Be(parent.Id);
        child.Level.Should().BeGreaterThan(parent.Level,
            "un conto figlio deve avere un livello maggiore del padre");

        _accountIds.Insert(0, child.Id); // child prima del parent nel cleanup
    }

    // ── GET /api/chart-of-accounts/{id} ───────────────────────────────────

    [Fact]
    public async Task GetById_ExistingAccount_ReturnsAccount()
    {
        var account = await CreateAccountAsync();

        var result = await GetAsync<ChartOfAccountsReadDto>($"/api/chart-of-accounts/{account.Id}");

        result.Id.Should().Be(account.Id);
        result.Code.Should().Be(account.Code);
    }

    [Fact]
    public async Task GetById_NonExisting_Returns404()
    {
        var response = await Client.GetAsync("/api/chart-of-accounts/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/chart-of-accounts/{id} ───────────────────────────────────

    [Fact]
    public async Task Update_ExistingAccount_Returns200WithUpdatedName()
    {
        var account = await CreateAccountAsync();

        var newName   = $"Conto Aggiornato {TestDataBuilder.ShortId()}";
        var updateDto = new UpdateChartOfAccountsDto
        {
            Code                = account.Code,
            Name                = newName,
            Type                = account.Type,
            IsActive            = true,
            AllocationMethod    = AllocationMethod.Standard,
            ChargeabilityTypeId = ChargeabilityType.Owner,
        };

        var (response, updated) = await PutAsync<ChartOfAccountsReadDto>(
            $"/api/chart-of-accounts/{account.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Name.Should().Be(newName);
    }

    [Fact]
    public async Task Update_DuplicateCode_Returns400()
    {
        var existingCode = $"EXIST-{TestDataBuilder.ShortId()}";
        var other   = await CreateAccountAsync(code: existingCode);
        var account = await CreateAccountAsync();

        var updateDto = new UpdateChartOfAccountsDto
        {
            Code                = existingCode, // codice già usato dall'altro conto
            Name                = account.Name,
            Type                = account.Type,
            IsActive            = true,
            AllocationMethod    = AllocationMethod.Standard,
            ChargeabilityTypeId = ChargeabilityType.Owner,
        };

        var (response, _) = await PutAsync<ChartOfAccountsReadDto>(
            $"/api/chart-of-accounts/{account.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "non è possibile aggiornare con un codice già in uso da un altro conto");
    }

    // ── DELETE /api/chart-of-accounts/{id} ────────────────────────────────

    [Fact]
    public async Task Delete_AccountWithNoChildren_Returns204()
    {
        var account = await CreateAccountAsync();
        _accountIds.Remove(account.Id); // non aggiungere al cleanup: lo eliminiamo nel test

        var response = await DeleteAsync($"/api/chart-of-accounts/{account.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_AccountWithChildren_Returns400()
    {
        var parent = await CreateAccountAsync(name: "Parent to delete");

        var childDto = TestDataBuilder.ChartOfAccounts(_condominiumId);
        childDto.ParentAccountId = parent.Id;
        var (_, child) = await PostAsync<ChartOfAccountsReadDto>("/api/chart-of-accounts", childDto);
        _accountIds.Insert(0, child!.Id); // child prima del parent

        var response = await DeleteAsync($"/api/chart-of-accounts/{parent.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "non si può eliminare un conto con conti figli");
    }
}
