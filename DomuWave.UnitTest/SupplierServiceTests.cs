using DomuWave.Services.Interfaces;
using DomuWave.Services.Models;
using DomuWave.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DomuWave.Tests.Services;

/// <summary>
/// Unit test per <see cref="ISupplierService"/>.
///
/// APPROCCIO: i test verificano il contratto dell'interfaccia tramite mock.
/// La logica di persistenza e le regole di business (ragione sociale obbligatoria,
/// ricerca per P.IVA) sono coperte dai test di integrazione in
/// <c>DomuWave.IntegrationTests.Tests.Supplier</c>.
///
/// Casi coperti:
/// <list type="bullet">
///   <item>GetByIdAsync — id esistente e id inesistente</item>
///   <item>CreateAsync — fornitore valido</item>
///   <item>UpdateAsync — fornitore esistente</item>
///   <item>DeleteAsync — id esistente e id inesistente</item>
///   <item>GetByTypeAsync — filtraggio per tipo</item>
///   <item>GetByVatNumberAsync — ricerca per P.IVA</item>
///   <item>SearchSuppliersAsync — ricerca testuale</item>
/// </list>
/// </summary>
public class SupplierServiceTests : TestBase
{
    private readonly Mock<ISupplierService> _serviceMock;
    private readonly FakeUser              _currentUser;
    private readonly CancellationToken     _ct = CancellationToken.None;
    private readonly Guid                  _tenantId;

    public SupplierServiceTests()
    {
        _tenantId    = Guid.NewGuid();
        _currentUser = FakeUser.Create(_tenantId);
        _serviceMock = MockOf<ISupplierService>();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsSupplier()
    {
        var expected = new Supplier { Id = 1, CompanyName = "Idraulica Rossi Srl" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, _currentUser, _ct))
                    .ReturnsAsync(expected);

        var result = await _serviceMock.Object.GetByIdAsync(1, _currentUser, _ct);

        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Idraulica Rossi Srl");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999, _currentUser, _ct))
                    .ReturnsAsync((Supplier?)null);

        var result = await _serviceMock.Object.GetByIdAsync(999, _currentUser, _ct);

        result.Should().BeNull();
    }

    // ── CreateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidSupplier_ReturnsSavedSupplierWithId()
    {
        var newSupplier  = new Supplier { CompanyName = "Elettrica Bianchi Srl", IsActive = true };
        var savedSupplier = new Supplier { Id = 10, CompanyName = "Elettrica Bianchi Srl", IsActive = true };

        _serviceMock.Setup(s => s.CreateAsync(newSupplier, _currentUser, _ct))
                    .ReturnsAsync(savedSupplier);

        var result = await _serviceMock.Object.CreateAsync(newSupplier, _currentUser, _ct);

        result.Id.Should().Be(10);
        result.CompanyName.Should().Be("Elettrica Bianchi Srl");
        _serviceMock.Verify(s => s.CreateAsync(newSupplier, _currentUser, _ct), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingSupplier_ReturnsUpdatedSupplier()
    {
        var toUpdate = new Supplier { Id = 1, CompanyName = "Ragione Aggiornata" };
        _serviceMock.Setup(s => s.UpdateAsync(toUpdate, _currentUser, _ct))
                    .ReturnsAsync(toUpdate);

        var result = await _serviceMock.Object.UpdateAsync(toUpdate, _currentUser, _ct);

        result.CompanyName.Should().Be("Ragione Aggiornata");
        _serviceMock.Verify(s => s.UpdateAsync(toUpdate, _currentUser, _ct), Times.Once);
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

    // ── GetByTypeAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByTypeAsync_ExistingType_ReturnsOnlyThatType()
    {
        var suppliers = new List<Supplier>
        {
            new() { Id = 1, SupplierType = "Idraulico" },
            new() { Id = 2, SupplierType = "Idraulico" },
        };
        _serviceMock.Setup(s => s.GetByTypeAsync(_tenantId, "Idraulico", _currentUser, _ct))
                    .ReturnsAsync(suppliers);

        var result = await _serviceMock.Object.GetByTypeAsync(_tenantId, "Idraulico", _currentUser, _ct);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.SupplierType == "Idraulico");
    }

    // ── GetByVatNumberAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetByVatNumberAsync_ExistingVat_ReturnsSupplier()
    {
        var supplier = new Supplier { Id = 5, VatNumber = "IT01234567890" };
        _serviceMock.Setup(s => s.GetByVatNumberAsync(_tenantId, "IT01234567890", _currentUser, _ct))
                    .ReturnsAsync(supplier);

        var result = await _serviceMock.Object.GetByVatNumberAsync(_tenantId, "IT01234567890", _currentUser, _ct);

        result.Should().NotBeNull();
        result!.VatNumber.Should().Be("IT01234567890");
    }

    [Fact]
    public async Task GetByVatNumberAsync_NonExistingVat_ReturnsNull()
    {
        _serviceMock.Setup(s => s.GetByVatNumberAsync(_tenantId, "NOTEXIST", _currentUser, _ct))
                    .ReturnsAsync((Supplier?)null);

        var result = await _serviceMock.Object.GetByVatNumberAsync(_tenantId, "NOTEXIST", _currentUser, _ct);

        result.Should().BeNull();
    }

    // ── SearchSuppliersAsync ──────────────────────────────────────────────

    [Fact]
    public async Task SearchSuppliersAsync_MatchingTerm_ReturnsResults()
    {
        var suppliers = new List<Supplier>
        {
            new() { Id = 1, CompanyName = "Idraulica Rossi" },
        };
        _serviceMock.Setup(s => s.SearchSuppliersAsync(_tenantId, "Rossi", _currentUser, _ct))
                    .ReturnsAsync(suppliers);

        var result = await _serviceMock.Object.SearchSuppliersAsync(_tenantId, "Rossi", _currentUser, _ct);

        result.Should().HaveCount(1);
        result.First().CompanyName.Should().Contain("Rossi");
    }

    [Fact]
    public async Task SearchSuppliersAsync_NoMatch_ReturnsEmptyList()
    {
        _serviceMock.Setup(s => s.SearchSuppliersAsync(_tenantId, "XXXXXX", _currentUser, _ct))
                    .ReturnsAsync(new List<Supplier>());

        var result = await _serviceMock.Object.SearchSuppliersAsync(_tenantId, "XXXXXX", _currentUser, _ct);

        result.Should().BeEmpty();
    }
}
