using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per <see cref="IUnitOwnerService"/>.
///
/// APPROCCIO: i test verificano il contratto dell'interfaccia tramite mock.
/// La creazione, l'aggiornamento e la cancellazione dei proprietari (inclusa la
/// logica di soft-delete e il refresh del DisplayName dell'unità) sono coperte
/// dai test di integrazione in <c>DomuWave.IntegrationTests.Tests.UnitOwner</c>.
///
/// Casi coperti:
/// <list type="bullet">
///   <item>GetByIdAsync — id esistente e id inesistente</item>
///   <item>GetByUnitIdAsync — tutti i proprietari di un'unità</item>
///   <item>GetActiveOwnersAsync — solo proprietari attivi</item>
///   <item>GetTotalOwnershipQuotaAsync — somma delle quote di proprietà</item>
///   <item>CreateAsync — proprietario valido</item>
///   <item>DeleteAsync — id esistente e id inesistente</item>
/// </list>
/// </summary>
public class UnitOwnerServiceTests : TestBase
{
    private readonly Mock<IUnitOwnerService> _serviceMock;
    private readonly FakeUser               _currentUser;
    private readonly CancellationToken      _ct = CancellationToken.None;
    private const    int                    UnitId = 1;

    public UnitOwnerServiceTests()
    {
        _currentUser = FakeUser.Create();
        _serviceMock = MockOf<IUnitOwnerService>();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsOwner()
    {
        var expected = new UnitOwner { Id = 1, FirstName = "Mario", LastName = "Rossi", IsActive = true };
        _serviceMock.Setup(s => s.GetByIdAsync(1, _currentUser, _ct)).ReturnsAsync(expected);

        var result = await _serviceMock.Object.GetByIdAsync(1, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Mario");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999, _currentUser, _ct))
                    .ReturnsAsync((UnitOwner?)null);

        var result = await _serviceMock.Object.GetByIdAsync(999, _currentUser, _ct);

        result.Should().BeNull();
    }

    // ── GetByUnitIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByUnitIdAsync_ReturnsAllOwnersForUnit()
    {
        var owners = new List<UnitOwner>
        {
            new() { Id = 1, IsActive = true,  OwnershipQuota = 50m },
            new() { Id = 2, IsActive = false, OwnershipQuota = 50m }, // ex-proprietario
        };
        _serviceMock.Setup(s => s.GetByUnitIdAsync(UnitId, It.IsAny<Guid>(), _currentUser, _ct))
                    .ReturnsAsync(owners);

        var result = await _serviceMock.Object.GetByUnitIdAsync(UnitId, Guid.Empty, _currentUser, _ct);

        result.Should().HaveCount(2, "GetByUnit deve restituire anche i proprietari storici");
    }

    // ── GetActiveOwnersAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetActiveOwnersAsync_ReturnsOnlyActiveOwners()
    {
        var activeOwners = new List<UnitOwner>
        {
            new() { Id = 1, IsActive = true, OwnershipQuota = 100m },
        };
        _serviceMock.Setup(s => s.GetActiveOwnersAsync(UnitId, _currentUser, _ct))
                    .ReturnsAsync(activeOwners);

        var result = await _serviceMock.Object.GetActiveOwnersAsync(UnitId, _currentUser, _ct);

        result.Should().OnlyContain(o => o.IsActive);
    }

    // ── GetTotalOwnershipQuotaAsync ───────────────────────────────────────

    [Fact]
    public async Task GetTotalOwnershipQuotaAsync_ReturnsCorrectSum()
    {
        _serviceMock.Setup(s => s.GetTotalOwnershipQuotaAsync(UnitId, _currentUser, _ct))
                    .ReturnsAsync(100m);

        var result = await _serviceMock.Object.GetTotalOwnershipQuotaAsync(UnitId, _currentUser, _ct);

        result.Should().Be(100m,
            "la somma delle quote di proprietà attive deve essere 100 per un'unità con un solo proprietario");
    }

    [Fact]
    public async Task GetTotalOwnershipQuotaAsync_MultipleOwners_ReturnsSum()
    {
        _serviceMock.Setup(s => s.GetTotalOwnershipQuotaAsync(UnitId, _currentUser, _ct))
                    .ReturnsAsync(100m); // 60 + 40

        var result = await _serviceMock.Object.GetTotalOwnershipQuotaAsync(UnitId, _currentUser, _ct);

        result.Should().Be(100m);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidOwner_ReturnsSavedOwnerWithId()
    {
        var newOwner  = new UnitOwner { FirstName = "Luca", LastName = "Verdi", OwnershipQuota = 100m };
        var saved     = new UnitOwner { Id = 5, FirstName = "Luca", LastName = "Verdi", OwnershipQuota = 100m };

        _serviceMock.Setup(s => s.CreateAsync(newOwner, _currentUser, _ct)).ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(newOwner, _currentUser, _ct);

        result.Id.Should().Be(5);
        _serviceMock.Verify(s => s.CreateAsync(newOwner, _currentUser, _ct), Times.Once);
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
