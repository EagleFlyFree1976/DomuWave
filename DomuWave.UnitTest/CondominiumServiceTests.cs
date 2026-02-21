using DomuWave.Domain.Models;
using DomuWave.Services.Interfaces;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per ICondominiumService.
/// </summary>
public class CondominiumServiceTests : TestBase
{
    private readonly Mock<ICondominiumService> _serviceMock;
    private readonly FakeUser _currentUser;
    private readonly Guid _tenantId;
    private readonly CancellationToken _ct = CancellationToken.None;

    public CondominiumServiceTests()
    {
        _tenantId = Guid.NewGuid();
        _currentUser = FakeUser.Create(_tenantId);
        _serviceMock = MockOf<ICondominiumService>();
    }

    // ─── GetByIdAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCondominium()
    {
        // Arrange
        var expected = new Condominium { Id = 1, Name = "Condominio Verde" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, _currentUser, _ct))
                    .ReturnsAsync(expected);

        // Act
        var result = await _serviceMock.Object.GetByIdAsync(1, _currentUser, _ct);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Condominio Verde");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999, _currentUser, _ct))
                    .ReturnsAsync((Condominium?)null);

        var result = await _serviceMock.Object.GetByIdAsync(999, _currentUser, _ct);

        result.Should().BeNull();
    }

    // ─── GetByTenantIdAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByTenantIdAsync_ReturnsCondominiumsForTenant()
    {
        var list = new List<Condominium>
        {
            new() { Id = 1, Name = "Cond A" },
            new() { Id = 2, Name = "Cond B" }
        };
        _serviceMock.Setup(s => s.GetByTenantIdAsync(_tenantId, _currentUser, _ct))
                    .ReturnsAsync(list);

        var result = await _serviceMock.Object.GetByTenantIdAsync(_tenantId, _currentUser, _ct);

        result.Should().HaveCount(2);
    }

    // ─── GetActiveCondominiumsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetActiveCondominiumsAsync_ReturnsOnlyActive()
    {
        var active = new List<Condominium>
        {
            new() { Id = 1, IsActive = true }
        };
        _serviceMock.Setup(s => s.GetActiveCondominiumsAsync(_tenantId, _currentUser, _ct))
                    .ReturnsAsync(active);

        var result = await _serviceMock.Object.GetActiveCondominiumsAsync(_tenantId, _currentUser, _ct);

        result.Should().OnlyContain(c => c.IsActive);
    }

    // ─── GetByCodeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsCondominium()
    {
        var cond = new Condominium { Id = 5, Code = "COD-001" };
        _serviceMock.Setup(s => s.GetByCodeAsync(_tenantId, "COD-001", _currentUser, _ct))
                    .ReturnsAsync(cond);

        var result = await _serviceMock.Object.GetByCodeAsync(_tenantId, "COD-001", _currentUser, _ct);

        result.Should().NotBeNull();
        result!.Code.Should().Be("COD-001");
    }

    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByCodeAsync(_tenantId, "INVALID", _currentUser, _ct))
                    .ReturnsAsync((Condominium?)null);

        var result = await _serviceMock.Object.GetByCodeAsync(_tenantId, "INVALID", _currentUser, _ct);

        result.Should().BeNull();
    }

    // ─── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidEntity_ReturnsSavedCondominium()
    {
        var newCond = new Condominium { Name = "Nuovo Condominio", Code = "NC-001" };
        var saved   = new Condominium { Id = 10, Name = "Nuovo Condominio", Code = "NC-001" };

        _serviceMock.Setup(s => s.CreateAsync(newCond, _currentUser, _ct))
                    .ReturnsAsync(saved);

        var result = await _serviceMock.Object.CreateAsync(newCond, _currentUser, _ct);

        result.Id.Should().Be(10);
        _serviceMock.Verify(s => s.CreateAsync(newCond, _currentUser, _ct), Times.Once);
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedCondominium()
    {
        var toUpdate = new Condominium { Id = 1, Name = "Nome Aggiornato" };
        _serviceMock.Setup(s => s.UpdateAsync(toUpdate, _currentUser, _ct))
                    .ReturnsAsync(toUpdate);

        var result = await _serviceMock.Object.UpdateAsync(toUpdate, _currentUser, _ct);

        result.Name.Should().Be("Nome Aggiornato");
        _serviceMock.Verify(s => s.UpdateAsync(toUpdate, _currentUser, _ct), Times.Once);
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1, _currentUser, _ct))
                    .ReturnsAsync(true);

        var result = await _serviceMock.Object.DeleteAsync(1, _currentUser, _ct);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        _serviceMock.Setup(s => s.DeleteAsync(999, _currentUser, _ct))
                    .ReturnsAsync(false);

        var result = await _serviceMock.Object.DeleteAsync(999, _currentUser, _ct);

        result.Should().BeFalse();
    }

    // ─── ExistsAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_ExistingId_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.ExistsAsync(1, _currentUser, _ct)).ReturnsAsync(true);

        var result = await _serviceMock.Object.ExistsAsync(1, _currentUser, _ct);

        result.Should().BeTrue();
    }

    // ─── GetPagedAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPagedAsync_ReturnsPaginatedResult()
    {
        var items = Enumerable.Range(1, 5)
                              .Select(i => new Condominium { Id = i })
                              .ToList();

        _serviceMock.Setup(s => s.GetPagedAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Condominium, bool>>>(),
                        1,
                        5, It.IsAny<System.Linq.Expressions.Expression<Func<Condominium, object>>>(), true, _currentUser, _ct))
                    .ReturnsAsync(((IList<Condominium>)items, 20));

        var (resultItems, total) = await _serviceMock.Object.GetPagedAsync(x => !x.IsDeleted,
            1,
            5, x => x.Name!, true, _currentUser, _ct);

        resultItems.Should().HaveCount(5);
        total.Should().Be(20);
    }
}
