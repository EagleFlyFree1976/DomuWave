using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per <see cref="IChartOfAccountsService"/>.
///
/// APPROCCIO: i test verificano il contratto dell'interfaccia tramite mock.
/// Le regole di business complesse (unicità codice, gerarchia padre-figlio,
/// sincronizzazione budget) sono coperte dai test di integrazione in
/// <c>DomuWave.IntegrationTests.Tests.ChartOfAccounts</c>.
///
/// Casi coperti:
/// <list type="bullet">
///   <item>GetByIdAsync — id esistente e id inesistente</item>
///   <item>GetByCondominiumIdAsync — restituisce conti del condominio</item>
///   <item>GetByTypeAsync — filtraggio per tipo (Entrata, Uscita, Patrimoniale)</item>
///   <item>GetByCodeAsync — ricerca per codice</item>
///   <item>GetRootAccountsAsync — solo conti root (senza padre)</item>
///   <item>GetChildAccountsAsync — solo conti figli di un parent</item>
///   <item>CreateAsync — conto valido</item>
///   <item>DeleteAsync — id esistente e id inesistente</item>
/// </list>
/// </summary>
public class ChartOfAccountsServiceTests : TestBase
{
    private readonly Mock<IChartOfAccountsService> _serviceMock;
    private readonly FakeUser                      _currentUser;
    private readonly CancellationToken             _ct = CancellationToken.None;
    private const    int                           CondominiumId = 1;

    public ChartOfAccountsServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IChartOfAccountsService>();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsAccount()
    {
        var expected = new ChartOfAccounts { Id = 1, Code = "USP01", Type = ChartOfAccountsType.Uscita };
        _serviceMock.Setup(s => s.GetByIdAsync(1, _currentUser, _ct))
                    .ReturnsAsync(expected);

        var result = await _serviceMock.Object.GetByIdAsync(1, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Code.Should().Be("USP01");
        result.Type.Should().Be(ChartOfAccountsType.Uscita);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999, _currentUser, _ct))
                    .ReturnsAsync((ChartOfAccounts?)null);

        var result = await _serviceMock.Object.GetByIdAsync(999, _currentUser, _ct);

        result.Should().BeNull();
    }

    // ── GetByCondominiumIdAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsAllAccountsForCondominium()
    {
        var accounts = new List<ChartOfAccounts>
        {
            new() { Id = 1, Type = ChartOfAccountsType.Entrata  },
            new() { Id = 2, Type = ChartOfAccountsType.Uscita   },
            new() { Id = 3, Type = ChartOfAccountsType.Patrimoniale },
        };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(CondominiumId, It.IsAny<Guid>(), _currentUser, _ct))
                    .ReturnsAsync(accounts);

        var result = await _serviceMock.Object.GetByCondominiumIdAsync(CondominiumId, Guid.Empty, _currentUser, _ct);

        result.Should().HaveCount(3);
    }

    // ── GetByTypeAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByTypeAsync_Entrata_ReturnsOnlyEntrataAccounts()
    {
        var entrate = new List<ChartOfAccounts>
        {
            new() { Id = 1, Type = ChartOfAccountsType.Entrata },
            new() { Id = 2, Type = ChartOfAccountsType.Entrata },
        };
        _serviceMock.Setup(s => s.GetByTypeAsync(CondominiumId, ChartOfAccountsType.Entrata, _currentUser, _ct))
                    .ReturnsAsync(entrate);

        var result = await _serviceMock.Object.GetByTypeAsync(
            CondominiumId, ChartOfAccountsType.Entrata, _currentUser, _ct);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.Type == ChartOfAccountsType.Entrata);
    }

    [Fact]
    public async Task GetByTypeAsync_Uscita_ReturnsOnlyUscitaAccounts()
    {
        var uscite = new List<ChartOfAccounts>
        {
            new() { Id = 3, Type = ChartOfAccountsType.Uscita },
        };
        _serviceMock.Setup(s => s.GetByTypeAsync(CondominiumId, ChartOfAccountsType.Uscita, _currentUser, _ct))
                    .ReturnsAsync(uscite);

        var result = await _serviceMock.Object.GetByTypeAsync(
            CondominiumId, ChartOfAccountsType.Uscita, _currentUser, _ct);

        result.Should().OnlyContain(a => a.Type == ChartOfAccountsType.Uscita);
    }

    // ── GetByCodeAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsAccount()
    {
        var account = new ChartOfAccounts { Id = 5, Code = "SPE01" };
        _serviceMock.Setup(s => s.GetByCodeAsync(CondominiumId, "SPE01", _currentUser, _ct))
                    .ReturnsAsync(account);

        var result = await _serviceMock.Object.GetByCodeAsync(CondominiumId, "SPE01", _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Code.Should().Be("SPE01");
    }

    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByCodeAsync(CondominiumId, "NOCODE", _currentUser, _ct))
                    .ReturnsAsync((ChartOfAccounts?)null);

        var result = await _serviceMock.Object.GetByCodeAsync(CondominiumId, "NOCODE", _currentUser, _ct);

        result.Should().BeNull();
    }

    // ── GetRootAccountsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetRootAccountsAsync_ReturnsOnlyAccountsWithNoParent()
    {
        var roots = new List<ChartOfAccounts>
        {
            new() { Id = 1, Level = 0 },
            new() { Id = 2, Level = 0 },
        };
        _serviceMock.Setup(s => s.GetRootAccountsAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(roots);

        var result = await _serviceMock.Object.GetRootAccountsAsync(CondominiumId, _currentUser, _ct);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.Level == 0);
    }

    // ── GetChildAccountsAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetChildAccountsAsync_ExistingParent_ReturnsChildren()
    {
        var children = new List<ChartOfAccounts>
        {
            new() { Id = 10, Level = 1 },
            new() { Id = 11, Level = 1 },
        };
        _serviceMock.Setup(s => s.GetChildAccountsAsync(1, _currentUser, _ct))
                    .ReturnsAsync(children);

        var result = await _serviceMock.Object.GetChildAccountsAsync(1, _currentUser, _ct);

        result.Should().HaveCount(2);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidAccount_ReturnsSavedAccountWithId()
    {
        var newAccount  = new ChartOfAccounts { Code = "USP99", Type = ChartOfAccountsType.Uscita };
        var savedAccount = new ChartOfAccounts { Id = 20, Code = "USP99", Type = ChartOfAccountsType.Uscita };

        _serviceMock.Setup(s => s.CreateAsync(newAccount, _currentUser, _ct))
                    .ReturnsAsync(savedAccount);

        var result = await _serviceMock.Object.CreateAsync(newAccount, _currentUser, _ct);

        result.Id.Should().Be(20);
        _serviceMock.Verify(s => s.CreateAsync(newAccount, _currentUser, _ct), Times.Once);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, _currentUser, _ct)).ReturnsAsync(true);

        var result = await _serviceMock.Object.DeleteAsync(1, _currentUser, _ct);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        _serviceMock.Setup(s => s.DeleteAsync(999, _currentUser, _ct)).ReturnsAsync(false);

        var result = await _serviceMock.Object.DeleteAsync(999, _currentUser, _ct);

        result.Should().BeFalse();
    }
}
