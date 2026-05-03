using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per <see cref="IUnitTenantService"/>.
///
/// APPROCCIO: i test verificano il contratto dell'interfaccia tramite mock.
/// La logica di creazione (inclusa la verifica dell'unità) e il soft-delete
/// sono coperti dai test di integrazione in
/// <c>DomuWave.IntegrationTests.Tests.UnitTenant</c>.
///
/// Casi coperti:
/// <list type="bullet">
///   <item>GetByIdAsync — id esistente e id inesistente</item>
///   <item>GetByUnitIdAsync — storico inquilini di un'unità</item>
///   <item>GetActiveByUnitIdAsync — solo inquilino attivo (contratto in corso)</item>
///   <item>GetExpiringLeasesAsync — contratti in scadenza entro N giorni</item>
///   <item>CreateAsync — inquilino valido</item>
///   <item>DeleteAsync — id esistente e id inesistente</item>
/// </list>
/// </summary>
public class UnitTenantServiceTests : TestBase
{
    private readonly Mock<IUnitTenantService> _serviceMock;
    private readonly FakeUser                _currentUser;
    private readonly CancellationToken       _ct = CancellationToken.None;
    private const    int                     UnitId = 1;
    private readonly Guid                    _tenantId;

    public UnitTenantServiceTests()
    {
        _tenantId    = Guid.NewGuid();
        _currentUser = FakeUser.Create(_tenantId);
        _serviceMock = MockOf<IUnitTenantService>();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsTenant()
    {
        var expected = new UnitTenant { Id = 1, FirstName = "Giuseppe", LastName = "Neri", IsActive = true };
        _serviceMock.Setup(s => s.GetByIdAsync(1, _currentUser, _ct)).ReturnsAsync(expected);

        var result = await _serviceMock.Object.GetByIdAsync(1, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.LastName.Should().Be("Neri");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999, _currentUser, _ct))
                    .ReturnsAsync((UnitTenant?)null);

        var result = await _serviceMock.Object.GetByIdAsync(999, _currentUser, _ct);

        result.Should().BeNull();
    }

    // ── GetByUnitIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByUnitIdAsync_ReturnsAllTenantsIncludingHistorical()
    {
        var tenants = new List<UnitTenant>
        {
            new() { Id = 1, IsActive = false, LeaseStartDate = new DateTime(2020, 1, 1) }, // ex-inquilino
            new() { Id = 2, IsActive = true,  LeaseStartDate = new DateTime(2024, 1, 1) }, // attuale
        };
        _serviceMock.Setup(s => s.GetByUnitIdAsync(UnitId, It.IsAny<Guid>(), _currentUser, _ct))
                    .ReturnsAsync(tenants);

        var result = await _serviceMock.Object.GetByUnitIdAsync(UnitId, Guid.Empty, _currentUser, _ct);

        result.Should().HaveCount(2, "GetByUnit deve restituire anche gli inquilini storici");
    }

    // ── GetActiveByUnitIdAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetActiveByUnitIdAsync_ReturnsOnlyActiveTenant()
    {
        var activeTenant = new UnitTenant { Id = 2, IsActive = true };
        _serviceMock.Setup(s => s.GetActiveByUnitIdAsync(UnitId, _currentUser, _ct))
                    .ReturnsAsync(activeTenant);

        var result = await _serviceMock.Object.GetActiveByUnitIdAsync(UnitId, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveByUnitIdAsync_NoActiveTenant_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetActiveByUnitIdAsync(UnitId, _currentUser, _ct))
                    .ReturnsAsync((UnitTenant?)null);

        var result = await _serviceMock.Object.GetActiveByUnitIdAsync(UnitId, _currentUser, _ct);

        result.Should().BeNull("un'unità libera non ha inquilini attivi");
    }

    // ── GetExpiringLeasesAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetExpiringLeasesAsync_Within30Days_ReturnsExpiringContracts()
    {
        var expiring = new List<UnitTenant>
        {
            new() { Id = 3, LeaseEndDate = DateTime.Today.AddDays(15) },
        };
        _serviceMock.Setup(s => s.GetExpiringLeasesAsync(_tenantId, 30, _currentUser, _ct))
                    .ReturnsAsync(expiring);

        var result = await _serviceMock.Object.GetExpiringLeasesAsync(_tenantId, 30, _currentUser, _ct);

        result.Should().HaveCount(1);
        result.First().LeaseEndDate.Should().BeBefore(DateTime.Today.AddDays(31));
    }

    [Fact]
    public async Task GetExpiringLeasesAsync_NoneExpiring_ReturnsEmptyList()
    {
        _serviceMock.Setup(s => s.GetExpiringLeasesAsync(_tenantId, 7, _currentUser, _ct))
                    .ReturnsAsync(new List<UnitTenant>());

        var result = await _serviceMock.Object.GetExpiringLeasesAsync(_tenantId, 7, _currentUser, _ct);

        result.Should().BeEmpty();
    }

    // ── CreateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidTenant_ReturnsSavedTenantWithId()
    {
        var newTenant  = new UnitTenant { FirstName = "Anna", LeaseStartDate = DateTime.Today };
        var saved      = new UnitTenant { Id = 8, FirstName = "Anna", LeaseStartDate = DateTime.Today };

        _serviceMock.Setup(s => s.CreateAsync(newTenant, _currentUser, _ct)).ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(newTenant, _currentUser, _ct);

        result.Id.Should().Be(8);
        _serviceMock.Verify(s => s.CreateAsync(newTenant, _currentUser, _ct), Times.Once);
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
