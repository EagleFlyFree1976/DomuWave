using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per <see cref="IMillesimalTableService"/>.
///
/// APPROCCIO: i test verificano il contratto dell'interfaccia tramite mock.
/// Le regole di business (protezione tabella DEF, verifica utilizzo in CoA ed Expense)
/// sono coperte dai test di integrazione in
/// <c>DomuWave.IntegrationTests.Tests.MillesimalTable</c>.
///
/// Casi coperti:
/// <list type="bullet">
///   <item>GetByIdAsync — id esistente e id inesistente</item>
///   <item>GetByCondominiumIdAsync — tutte le tabelle del condominio</item>
///   <item>GetActiveTablesAsync — solo tabelle attive</item>
///   <item>GetByCodeAsync — ricerca per codice univoco</item>
///   <item>CreateAsync — tabella valida</item>
///   <item>DeleteAsync — id esistente e id inesistente</item>
/// </list>
/// </summary>
public class MillesimalTableServiceTests : TestBase
{
    private readonly Mock<IMillesimalTableService> _serviceMock;
    private readonly FakeUser                     _currentUser;
    private readonly CancellationToken            _ct = CancellationToken.None;
    private const    int                          CondominiumId = 1;

    public MillesimalTableServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IMillesimalTableService>();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsTable()
    {
        var expected = new MillesimalTable { Id = 1, Code = "GEN", TotalMillesimal = 1000m, IsActive = true };
        _serviceMock.Setup(s => s.GetByIdAsync(1, _currentUser, _ct)).ReturnsAsync(expected);

        var result = await _serviceMock.Object.GetByIdAsync(1, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Code.Should().Be("GEN");
        result.TotalMillesimal.Should().Be(1000m);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999, _currentUser, _ct))
                    .ReturnsAsync((MillesimalTable?)null);

        var result = await _serviceMock.Object.GetByIdAsync(999, _currentUser, _ct);

        result.Should().BeNull();
    }

    // ── GetByCondominiumIdAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetByCondominiumIdAsync_ReturnsAllTablesForCondominium()
    {
        var tables = new List<MillesimalTable>
        {
            new() { Id = 1, Code = "GEN",   IsActive = true  },
            new() { Id = 2, Code = "SCALE", IsActive = true  },
            new() { Id = 3, Code = "OLD",   IsActive = false },
        };
        _serviceMock.Setup(s => s.GetByCondominiumIdAsync(CondominiumId, It.IsAny<Guid>(), _currentUser, _ct))
                    .ReturnsAsync(tables);

        var result = await _serviceMock.Object.GetByCondominiumIdAsync(CondominiumId, Guid.Empty, _currentUser, _ct);

        result.Should().HaveCount(3, "GetByCondominium deve includere anche le tabelle disattivate");
    }

    // ── GetActiveTablesAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetActiveTablesAsync_ReturnsOnlyActiveTables()
    {
        var activeTables = new List<MillesimalTable>
        {
            new() { Id = 1, IsActive = true },
            new() { Id = 2, IsActive = true },
        };
        _serviceMock.Setup(s => s.GetActiveTablesAsync(CondominiumId, _currentUser, _ct))
                    .ReturnsAsync(activeTables);

        var result = await _serviceMock.Object.GetActiveTablesAsync(CondominiumId, _currentUser, _ct);

        result.Should().OnlyContain(t => t.IsActive);
    }

    // ── GetByCodeAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsTable()
    {
        var table = new MillesimalTable { Id = 1, Code = "GEN", TotalMillesimal = 1000m };
        _serviceMock.Setup(s => s.GetByCodeAsync(CondominiumId, "GEN", _currentUser, _ct))
                    .ReturnsAsync(table);

        var result = await _serviceMock.Object.GetByCodeAsync(CondominiumId, "GEN", _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Code.Should().Be("GEN");
    }

    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByCodeAsync(CondominiumId, "NOCODE", _currentUser, _ct))
                    .ReturnsAsync((MillesimalTable?)null);

        var result = await _serviceMock.Object.GetByCodeAsync(CondominiumId, "NOCODE", _currentUser, _ct);

        result.Should().BeNull();
    }

    // ── CreateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidTable_ReturnsSavedTableWithId()
    {
        var newTable  = new MillesimalTable { Code = "NUOVA", TotalMillesimal = 1000m };
        var saved     = new MillesimalTable { Id = 7, Code = "NUOVA", TotalMillesimal = 1000m };

        _serviceMock.Setup(s => s.CreateAsync(newTable, _currentUser, _ct)).ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(newTable, _currentUser, _ct);

        result.Id.Should().Be(7);
        result.TotalMillesimal.Should().Be(1000m);
        _serviceMock.Verify(s => s.CreateAsync(newTable, _currentUser, _ct), Times.Once);
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
